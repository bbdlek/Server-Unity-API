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
using HeliosVariable = MVS.Realtime.HeliosVariable;
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
                    if (!heliosAttributes[i].GetValue(this).Equals(initialHeliosValues[i]))
                        b = true;
                }
                return b;
            }
        }

        public List<FieldInfo> heliosAttributes = new List<FieldInfo>();
        public List<object> initialHeliosValues = new List<object>();
        
        public List<HeliosVariable> HeliosVariableTable = new List<HeliosVariable>();

        public Dictionary<string, Tuple<MethodInfo, string>> RPCMethods =
            new Dictionary<string, Tuple<MethodInfo, string>>();

        // [SerializeField] private uint _instanceID;
        //
        // public uint instanceID
        // {
        //     get => ObjectInfo.ObjectID.InstanceID;
        //     set
        //     {
        //         _instanceID = value;
        //         ObjectInfo.ObjectID.InstanceID = value;
        //     }
        // }
        //
        // [SerializeField] private uint _clientInstanceID;
        //
        // public uint clientInstanceID
        // {
        //     get => ObjectInfo.ObjectID.ClientInstanceID;
        //     set
        //     {
        //         _clientInstanceID = value;
        //         ObjectInfo.ObjectID.ClientInstanceID = value;
        //     }
        // }
        
        public ObjectInfo ObjectInfo = new ObjectInfo
        {
            ObjectID = new ObjectID(),
            SyncType = ObjectSyncType.GlobalOwn,
            OwnerPlayerID = 0,
            CustomValues = null
        };
        
        protected bool isMine = false;
        
        public bool IsMine
        {
            get { return isMine; }
            set { isMine = value; }
        }
        
        public virtual void Start()
        {
            if(ObjectInfo == null)
                ObjectInfo = new ObjectInfo();
             
            FindNetworkedVariables();
            FindHeliosVariable();
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

        public void RPC(string methodName)
        {
            Debug.Log(ObjectInfo.ObjectID.InstanceID);
            HeliosNetwork.RPC(ObjectInfo.ObjectID.InstanceID, methodName);
        }

        public void ExecuteRpc(string methodName)
        {
            RPCMethods[methodName].Item1.Invoke(this, null);
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
                        ho.initialHeliosValues.Add(DeepCopyHelper.DeepCopy(obj));
                        
                        //HELIOSVARIABLE
                        ho.ObjectInfo.CustomData.Add(hv.ToByteString());
                    }
                    else
                    {
                        attribute.Owner = this;
                        
                        //ATTRIBUTE
                        heliosAttributes.Add(field);
                        initialHeliosValues.Add(DeepCopyHelper.DeepCopy(obj));
                        
                        //HELIOSVARIABLE
                        ObjectInfo.CustomData.Add(hv.ToByteString());
                    }
                }
            }
        }

        public void FindHeliosVariable()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(HeliosVariable).IsAssignableFrom(field.FieldType))
                {
                    HeliosVariable heliosVariable = (HeliosVariable)field.GetValue(this);
                    if (GetComponent<HeliosObject>())
                    {
                        var ho = GetComponent<HeliosObject>();
                        ho.HeliosVariableTable.Add(heliosVariable);
                        heliosVariable.SetOwner(ho);
                        heliosVariable.SetIndex(ho.HeliosVariableTable.LastIndexOf(heliosVariable)); 
                        // ho.ObjectInfo.TestValues.Add(heliosVariable.GetValue());
                        ho.ObjectInfo.CustomData.Add(heliosVariable.GetValue().ToByteString());
                    }
                    else
                    {
                        HeliosVariableTable.Add(heliosVariable);
                        heliosVariable.SetOwner(this);
                        heliosVariable.SetIndex(HeliosVariableTable.LastIndexOf(heliosVariable));
                        // ObjectInfo.TestValues.Add(heliosVariable.GetValue()); 
                        ObjectInfo.CustomData.Add(heliosVariable.GetValue().ToByteString());
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
                        GetComponent<HeliosObject>().RPCMethods.Add(methodName, Tuple.Create(method, target));
                    }
                    else
                    {
                        string methodName = method.Name;
                        string target = attribute.Target;
                        RPCMethods.Add(methodName, Tuple.Create(method, target));
                    }
                }
            }
        }

        public void UpdateCustomData()
        {
            HeliosMonoBehavior owner;
            if (GetComponent<HeliosObject>())
            {
                var ho = GetComponent<HeliosObject>();
                owner = ho;
            }
            else
            {
                owner = this;
            }
            for (int i = 0; i < ObjectInfo.CustomData.Count; i++)
            {
                FieldInfo field = heliosAttributes[i];
                switch (Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).ValueCase)
                {
                    case Protocol.HeliosVariable.ValueOneofCase.NInt32:
                        field.SetValue(owner, Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NInt32);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NInt64:
                        field.SetValue(owner, Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NInt64); 
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NFloat:
                        field.SetValue(owner, Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NFloat);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NDouble:
                        field.SetValue(owner, Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NDouble);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NString:
                        field.SetValue(owner, Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NString);
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

                initialHeliosValues[i] = field.GetValue(owner);
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
            foreach (var obj in HeliosNetwork.HeliosObjectList)
            {
                obj.ObjectInfo.SyncType = ObjectSyncType.GlobalOwn;
                obj.ObjectInfo.OwnerPlayerID = 0;
                var pkt = new C_ADD_NETWORK_OBJECTS();
                pkt.ObjectInfos.Add(obj.ObjectInfo);
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_ADD_NETWORK_OBJECTS, pkt);
            }
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
