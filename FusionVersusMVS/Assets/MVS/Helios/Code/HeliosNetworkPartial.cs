using System.Collections.Generic;
using System.Linq;
using _1_Scripts._8_HeliosTest;
using MVS.Realtime;
using Protocol;
using Unity.VisualScripting;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using OperationCode = MVS.Realtime.OperationCode;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public static partial class HeliosNetwork
    {
        public static Dictionary<uint, HeliosObject> HeliosObjectList = new Dictionary<uint, HeliosObject>();
        
        // public static Dictionary<uint, HeliosObject> MyHeliosObjectList = new Dictionary<uint, HeliosObject>();

        public static Queue<HeliosObject> MyHeliosObjectQueue = new Queue<HeliosObject>();
        
        public static void AddCallbackTarget(object target)
        {
            RealtimeClient.AddCallbackTarget(target);
        }
        
        public static void RemoveCallbackTarget(object target)
        {
            RealtimeClient.RemoveCallbackTarget(target);
        }

        private static IHeliosPrefabPool _prefabPool;

        public static IHeliosPrefabPool PrefabPool
        {
            get => _prefabPool;
            set
            {
                if (value == null)
                {
                    _prefabPool = new DefaultPrefabPool();
                }
                else
                {
                    _prefabPool = value;
                }
            }
        }

        private static void OnEvent(EventData eventData)
        {
            Debug.Log(eventData.code);
            switch (eventData.code)
            {
                case EventCode.PKT_S_INITIAL_OBJECTS:
                    var InitData = Packs.Parser.ParseFrom(eventData.FixedData).SInitialObjects;
                    foreach (var objectInfo in InitData.ObjectInfos)
                    {
                        NetworkInstantiate(objectInfo);
                    }
                    break;
                case EventCode.PKT_S_ADD_NETWORK_OBJECTS:
                    var data = Packs.Parser.ParseFrom(eventData.FixedData).SAddNetworkObjects;
                    foreach (var objectInfo in data.ObjectInfos)
                    {
                        if(objectInfo.OwnerPlayerID != LocalPlayer.UserId)
                        {
                            NetworkInstantiate(objectInfo);
                        }
                        else
                        {
                            var obj = MyHeliosObjectQueue.Dequeue();
                            obj.instanceId = objectInfo.ObjectID.InstanceID;
                            HeliosObjectList[obj.instanceId] = obj;
                            foreach (var heliosMonoBehavior in obj.GetComponentsInChildren<HeliosMonoBehavior>())
                            {
                                heliosMonoBehavior.FindHeliosVariable();
                                heliosMonoBehavior.SetHeliosVariableIndex();
                            }
                            Debug.Log(HeliosObjectList[obj.instanceId].GetComponent<HeliosObject>().instanceId);
                        }
                    }
                    break;
                case EventCode.PKT_S_UPDATE_NETWORK_OBJECTS:
                    var dataUpdate = Packs.Parser.ParseFrom(eventData.FixedData).SUpdateNetworkObjects;
                    Debug.Log(dataUpdate.ObjectInfos.Count);
                    foreach (var objectInfo in dataUpdate.ObjectInfos)
                    {
                        var id = objectInfo.ObjectID.InstanceID;
                        //Find Network Objects -> Update Transform
                        NetworkUpdateObject(id, objectInfo);
                    }
                    break;
                case EventCode.PKT_S_REMOVE_NETWORK_OBJECTS:
                    var dataRemove = Packs.Parser.ParseFrom(eventData.FixedData).SRemoveNetworkObjects;
                    foreach (var objectInfo in dataRemove.ObjectInfos)
                    {
                        var id = objectInfo.ObjectID.InstanceID;
                        NetworkRemoveObject(id);
                    }
                    break;
                
                case CustomEventCode.Variable:
                    var variable = Packs.Parser.ParseFrom(eventData.FixedData).CVariable;
                    Debug.Log(variable.Index);
                    Debug.Log(variable.HeliosVariable.NInt32);
                    if (variable.HeliosVariable.ValueCase is HeliosVariable.ValueOneofCase.NInt32)
                        HeliosVariableDic[(int)variable.Index].ConvertTo<HNInt>().Value = variable.HeliosVariable.NInt32;
                    break;
            }
        }

        private static void OnOperation(OperationResponse opRes)
        {
            Debug.Log(opRes.OperationCode);
            switch (opRes.OperationCode)
            {
                case OperationCode.HEART_BEAT:
                    break;
                case OperationCode.ROOM_JOIN_OR_CREATE:
                    break;
                case OperationCode.GROUP_JOIN:
                    break;
            }
        }

        private static void OnClientStateChanged(ClientState arg1, ClientState arg2)
        {
            RealtimeClient.RealtimePeer.Listener.MVSDebug(DebugLevel.INFO, arg2.ToString());
            switch (arg2)
            {
                case ClientState.ConnectedToMVS:
                    RealtimeClient.ConnectionCallbacksTarget.OnConnected();
                    break;
                case ClientState.DisConnected:
                    break;
            }
        }

        public static void Service()
        {
            RealtimeClient.Service();
        }

        public static void RemoveMyObjects()
        {
            foreach (var obj in HeliosObjectList.Values)
            {
                if(obj.GetComponent<HeliosTransform>().IsMine)
                    NetworkRemoveObject(obj.instanceId);
            }
        }
    }
}