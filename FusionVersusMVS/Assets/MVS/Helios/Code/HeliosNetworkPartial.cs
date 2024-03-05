using System.Collections.Generic;
using System.Linq;
using MVS.Realtime;
using Protocol;
using UnityEngine;
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
                case EventCode.PKT_S_GROUP_JOIN:
                    var InitialObjPkt = new C_INITIAL_OBJECTS();
                    RaiseEvent(EventCode.PKT_C_INITIAL_OBJECTS, InitialObjPkt);
                    break;
                case EventCode.PKT_S_INITIAL_OBJECTS:
                    var InitData = S_INITIAL_OBJECTS.Parser.ParseFrom(eventData.Data);
                    foreach (var objectInfo in InitData.ObjectInfos)
                    {
                        NetworkInstantiate(objectInfo);
                    }
                    break;
                case EventCode.PKT_S_ADD_NETWORK_OBJECTS:
                    var data = S_ADD_NETWORK_OBJECTS.Parser.ParseFrom(eventData.Data);
                    Debug.Log(data.ObjectInfos.Count);
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
                            Debug.Log(HeliosObjectList[obj.instanceId].GetComponent<HeliosObject>().instanceId);
                        }
                    }
                    break;
                case EventCode.PKT_S_UPDATE_NETWORK_OBJECTS:
                    var dataUpdate = S_UPDATE_NETWORK_OBJECTS.Parser.ParseFrom(eventData.Data);
                    foreach (var objectInfo in dataUpdate.ObjectInfos)
                    {
                        var id = objectInfo.ObjectID.InstanceID;
                        //Find Network Objects -> Update Transform
                        NetworkUpdateObject(id, objectInfo);
                    }
                    break;
                case EventCode.PKT_S_REMOVE_NETWORK_OBJECTS:
                    var dataRemove = S_REMOVE_NETWORK_OBJECTS.Parser.ParseFrom(eventData.Data);
                    foreach (var objectInfo in dataRemove.ObjectInfos)
                    {
                        var id = objectInfo.ObjectID.InstanceID;
                        NetworkRemoveObject(id);
                    }
                    break;
            }
        }

        private static void OnOperation(OperationResponse obj)
        {
            
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