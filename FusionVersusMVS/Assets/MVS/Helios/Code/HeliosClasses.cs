using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using _1_Scripts._8_HeliosTest;
using Google.Protobuf;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = MVS.Realtime.HeliosVariable;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public class HeliosMonoBehavior : MonoBehaviour
    {
        public bool hasUpdate = false;
        
        public List<HeliosVariable> HeliosVariableTable = new List<HeliosVariable>();

        public uint instanceID;
        
        public uint clientInstanceID;
        
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

        private void FixedUpdate()
        {
            instanceID = ObjectInfo.ObjectID.InstanceID;
            clientInstanceID = ObjectInfo.ObjectID.ClientInstanceID;
        }

        public virtual void Start()
        {
            if(ObjectInfo == null)
                ObjectInfo = new ObjectInfo();
             
            FindHeliosVariable();
            
            if(!GetComponentInChildren<HeliosObject>())
            {
                HeliosNetwork.HeliosObjectList.Add(this);
                ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(this);
                Debug.Log("ClientInstanceID: " + ObjectInfo.ObjectID.ClientInstanceID);   
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

        public void UpdateCustomData()
        {
            for (int i = 0; i < ObjectInfo.CustomData.Count; i++)
            {
                switch (Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).ValueCase)
                {
                    case Protocol.HeliosVariable.ValueOneofCase.NInt32:
                        if (HeliosVariableTable[i] is HNInt hnIntVariable)
                            hnIntVariable.Value =
                                Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NInt32;
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NInt64:
                        if (HeliosVariableTable[i] is HNLong hnLongVariable)
                            hnLongVariable.Value =
                                Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NInt64;
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NFloat:
                        if (HeliosVariableTable[i] is HNFloat hnFloatVariable)
                            hnFloatVariable.Value =
                                Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NFloat;
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NDouble:
                        if (HeliosVariableTable[i] is HNDouble hnDoubleVariable)
                            hnDoubleVariable.Value =
                                Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NDouble;
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NString:
                        if (HeliosVariableTable[i] is HNString hnStringVariable)
                            hnStringVariable.Value =
                                Protocol.HeliosVariable.Parser.ParseFrom(ObjectInfo.CustomData[i]).NString;
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
                HeliosVariableTable[i].SetFlag(false);
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
