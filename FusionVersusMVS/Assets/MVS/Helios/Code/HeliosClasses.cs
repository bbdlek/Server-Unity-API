using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Google.Protobuf;
using MVS.Helios.Utility;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public class HeliosMonoBehavior : MonoBehaviour
    {
        public bool hasUpdate
        {
            get
            {
                bool b = false;
                for (int i = 0; i < heliosAttributes.Count; i++)
                {
                    if (!heliosAttributes[i].GetValue(attributeMonoBehaviors[i]).Equals(initialHeliosValues[i]))
                        b = true;
                }
                return b;
            }
        }

        public List<FieldInfo> heliosAttributes = new List<FieldInfo>();
        public List<HeliosMonoBehavior> attributeMonoBehaviors = new List<HeliosMonoBehavior>();
        public List<object> initialHeliosValues = new List<object>();

        public Dictionary<ulong, Tuple<MethodInfo, HeliosMonoBehavior, string>> RPCMethods =
            new Dictionary<ulong, Tuple<MethodInfo, HeliosMonoBehavior, string>>();
        

        public ObjectInfo ObjectInfo = new ObjectInfo
        {
            ObjectID = new ObjectID(),
            SyncType = ObjectSyncType.GlobalOwn,
            OwnerPlayerID = 0,
            CustomValues = null
        };
        
        public bool IsMine
        {
            get
            {
                if (GetComponent<HeliosObject>())
                {
                    return GetComponent<HeliosObject>().IsMine;
                }
                return false;
            }
        }
        
        public virtual void Awake()
        {
            if(ObjectInfo == null)
                ObjectInfo = new ObjectInfo();
             
            FindNetworkedVariables();
            FindRPCMethods();
            
            if(!GetComponentInChildren<HeliosObject>())
            {
                HeliosNetwork.HeliosObjectList.Add(this);
                ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(this);
            }
        }

        private ByteString ObjectToBytes(object obj)
        {
            int iSize = Marshal.SizeOf(obj);

            byte[] arr = new byte[iSize];

            IntPtr ptr = Marshal.AllocHGlobal(iSize);
            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, iSize);
            Marshal.FreeHGlobal(ptr);
            
            return ByteString.CopyFrom(arr);
        }

        private T ByteToObject<T>(ByteString buffer)
        {
            int size = Marshal.SizeOf(typeof(T));

            if (size > buffer.Length)
            {
                throw new Exception();
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer.ToByteArray(), 0, ptr, size);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return obj;
        }

        public void RPC(string methodName, params object[] args)
        {
            // uint instanceID;
            // if (GetComponent<HeliosObject>())
            // {
            //     instanceID = GetComponent<HeliosObject>().InstanceId;
            // }
            // else
            // {
            //     instanceID = ObjectInfo.ObjectID.InstanceID;
            // }
            HeliosNetwork.RPC(ObjectInfo.ObjectID.InstanceID, methodName, args);
        }

        public void ExecuteRpc(ulong methodNameHash, byte[] methodArgs)
        {
            var parameters = HeliosUtility.DeserializeParameters(methodArgs);
            var method = RPCMethods[methodNameHash].Item1;
            var ho = RPCMethods[methodNameHash].Item2;
            try
            {
                method.Invoke(ho, parameters);
            }
            catch (Exception e)
            {
                Debug.LogError($"예상된 매개변수 수와 일치하지 않습니다. {e.Message}");
                throw;
            }
        }

        public void FindNetworkedVariables()
        {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                HNSyncAttribute attribute =
                    (HNSyncAttribute)Attribute.GetCustomAttribute(field, typeof(HNSyncAttribute));
                if (attribute != null)
                {
                    Protocol.HeliosVariable hv = new Protocol.HeliosVariable();
                        
                    if (field.FieldType == typeof(int))
                    {
                        hv.NInt32 = (int)field.GetValue(this);
                    }
                    else if(field.FieldType == typeof(long))
                    {
                        hv.NInt64 = (long)field.GetValue(this);
                    }
                    else if(field.FieldType == typeof(float))
                    {
                        hv.NFloat = (float)field.GetValue(this);
                    }
                    else if(field.FieldType == typeof(double))
                    {
                        hv.NDouble = (double)field.GetValue(this);
                    }
                    else if(field.FieldType == typeof(bool))
                    {
                        hv.NBool = (bool)field.GetValue(this);
                    }
                    else if(field.FieldType == typeof(string))
                    {
                        hv.NString = (string)field.GetValue(this);
                    }
                    else if(field.FieldType == typeof(Vector3))
                    {
                        Vector3 v = (Vector3)field.GetValue(this);
                        hv.NVector.X = v.x;
                        hv.NVector.Y = v.y;
                        hv.NVector.Z = v.z;
                    }
                    else if(field.FieldType == typeof(Quaternion))
                    {
                        Quaternion q = (Quaternion)field.GetValue(this);
                        Vector3 v = q.eulerAngles;
                        hv.NVector.X = v.x;
                        hv.NVector.Y = v.y;
                        hv.NVector.Z = v.z;
                    }
                    var obj = field.GetValue(this);
                    if (GetComponent<HeliosObject>())
                    {
                        var ho = GetComponent<HeliosObject>();
                        attribute.Owner = ho;
                        
                        //ATTRIBUTE
                        ho.heliosAttributes.Add(field);
                        ho.attributeMonoBehaviors.Add(this);
                        ho.initialHeliosValues.Add(DeepCopyHelper.DeepCopy(obj));
                        
                        //HELIOSVARIABLE
                        ho.ObjectInfo.CustomData.Add(hv.ToByteString());
                    }
                    else
                    {
                        attribute.Owner = this;
                        
                        //ATTRIBUTE
                        heliosAttributes.Add(field);
                        attributeMonoBehaviors.Add(this);
                        initialHeliosValues.Add(DeepCopyHelper.DeepCopy(obj));
                        
                        //HELIOSVARIABLE
                        ObjectInfo.CustomData.Add(hv.ToByteString());
                    }
                }
            }
        }

        public void FindRPCMethods()
        {
            Type classType = GetType();
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                HeliosRPCAttribute attribute =
                    (HeliosRPCAttribute)Attribute.GetCustomAttribute(method, typeof(HeliosRPCAttribute));
                if (attribute != null)
                {
                    if (GetComponent<HeliosObject>())
                    {
                        string methodName = method.Name;
                        string target = attribute.Target;
                        var hash = HeliosUtility.Compute64BitHash(methodName);
                        
                        GetComponent<HeliosObject>().RPCMethods.Add(hash, Tuple.Create(method, this, target));
                    }
                    else
                    {
                        string methodName = method.Name;
                        string target = attribute.Target;
                        var hash = HeliosUtility.Compute64BitHash(methodName);
                       
                        RPCMethods.Add(hash, Tuple.Create(method, this, target));
                    }
                }
            }
        }

        public void UpdateCustomData()
        {
            HeliosMonoBehavior owner;
            for (int i = 0; i < ObjectInfo.CustomData.Count; i++)
            {
                FieldInfo field = heliosAttributes[i];
                switch (Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).ValueCase)
                {
                    case Protocol.HeliosVariable.ValueOneofCase.NInt32:
                        field.SetValue(attributeMonoBehaviors[i], Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NInt32);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NInt64:
                        field.SetValue(attributeMonoBehaviors[i], Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NInt64); 
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NFloat:
                        field.SetValue(attributeMonoBehaviors[i], Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NFloat);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NBool:
                        field.SetValue(attributeMonoBehaviors[i], Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NBool);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NDouble:
                        field.SetValue(attributeMonoBehaviors[i], Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NDouble);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NString:
                        field.SetValue(attributeMonoBehaviors[i], Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NString);
                        break;
                    // case Protocol.HeliosVariable.ValueOneofCase.NVector:
                    //     if (HeliosVariableTable[i] is HNVector hnVectorVariable)
                    //         hnVectorVariable.Value =
                    //             Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NVector;
                    //     else if (HeliosVariableTable[i] is HNQuaternion hnQuaternionVariable)
                    //         hnQuaternionVariable.Value =
                    //             Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NVector;
                    //     break;
                }

                initialHeliosValues[i] = field.GetValue(attributeMonoBehaviors[i]);
            }
        }
    }

    public class DefaultPrefabPool : IHeliosPrefabPool
    {
        public readonly Dictionary<uint, GameObject> GOCache = new Dictionary<uint, GameObject>();
        
        public GameObject Instantiate(uint prefabId, Vector3 position, Quaternion rotation)
        {
            GameObject go = null;
            bool cached = GOCache.TryGetValue(prefabId, out go);
            if (!cached)
            {
                go = HeliosNetwork.HeliosSettings.NetworkPrefabs.FindPrefabByNetworkId(prefabId).gameObject;
                if (go == null)
                {
                    Debug.LogError($"");
                }
                else
                {
                    GOCache.Add(prefabId, go);
                }
            }

            bool isActive = go.activeSelf;
            if(isActive) go.SetActive(false);

            GameObject instance = GameObject.Instantiate(go, position, rotation);
            
            if(isActive) go.SetActive(true);
            return instance;
        }

        public void Destroy(GameObject gameObject)
        {
            // 내 것만 ?
            var RemovePkt = new C_REMOVE_NETWORK_OBJECTS();
            ObjectInfo objectInfo = new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = 0,
                    InstanceID = gameObject.GetComponent<HeliosObject>().InstanceId
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
            };
            RemovePkt.ObjectInfos.Add(objectInfo);
            HeliosNetwork.RaiseEvent(EventCode.PKT_C_REMOVE_NETWORK_OBJECTS, RemovePkt);
            HeliosNetwork.HeliosObjectList.RemoveAt((int)gameObject.GetComponent<HeliosObject>().InstanceId);
            GameObject.Destroy(gameObject);
        }
        
        public void Destroy(uint id)
        {
            var obj = HeliosNetwork.HeliosObjectList.Find(x => x.ObjectInfo.ObjectID.InstanceID == id);
            var RemovePkt = new C_REMOVE_NETWORK_OBJECTS();
            ObjectInfo objectInfo = obj.ObjectInfo;
            RemovePkt.ObjectInfos.Add(objectInfo);
            HeliosNetwork.RaiseEvent(EventCode.PKT_C_REMOVE_NETWORK_OBJECTS, RemovePkt);
            GameObject.Destroy(obj.gameObject);
        }
    }

    public class MonoBehaviorHeliosCallbacks : HeliosMonoBehavior, IConnectionCallbacks, IMakingRoomCallbacks,
        IInRoomCallbacks, IMakingGroupCallbacks, IInGroupCallbacks, IOnEventCallbacks, IErrorInfoCallbacks
    {
        public virtual void OnEnable()
        {
            HeliosNetwork.AddCallbackTarget(this);
        }
        
        public virtual void OnDisable()
        {
            HeliosNetwork.RemoveCallbackTarget(this);
        }

        public virtual void OnConnected()
        {
        }

        public virtual void OnConnectedToMaster()
        {
        }

        public virtual void OnDisconnected()
        {
            HeliosNetwork.RemoveMyObjects();
        }

        public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public virtual void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        public virtual void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
        }

        public virtual void OnCreatedRoomFailed(short failCode, string message)
        {
        }

        public virtual void OnJoinedRoom()
        {
        }

        public virtual void OnJoinedRoomFailed(short failCode, string message)
        {
        }

        public virtual void OnLeftRoom()
        {
        }

        public virtual void OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        public virtual void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public virtual void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public virtual void OnCreatedGroup()
        {
            
        }

        public virtual void OnCreatedGroupFailed(short failCode, string message)
        {
        }

        public virtual void OnJoinedGroup()
        {
        }

        public virtual void OnJoinedGroupFailed(short failCode, string message)
        {
        }

        public virtual void OnLeftGroup()
        {
        }

        public virtual void OnPlayerEnteredGroup(Player newPlayer)
        {
        }

        public virtual void OnPlayerLeftGroup(Player otherPlayer)
        {
        }

        public virtual void OnEvent(EventData eventData)
        {
        }

        public virtual void OnErrorInfo()
        {
        }
    }
}
