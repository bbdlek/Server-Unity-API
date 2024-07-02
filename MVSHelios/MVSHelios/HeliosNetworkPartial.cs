using System.Collections.Generic;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using OperationCode = MVS.Realtime.OperationCode;

namespace MVS.Helios
{
    public static partial class HeliosNetwork
    {
        public static List<HeliosMonoBehavior> HeliosObjectList = new List<HeliosMonoBehavior>();

        public static HeliosMonoBehavior FindObjectById(uint id)
        {
            return HeliosObjectList.Find(x => x.ObjectInfo.ObjectID.InstanceID == (int)id);
        }
        
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
            switch (eventData.code)
            {
                case EventCode.PKT_S_INITIAL_OBJECTS:
                    var InitData = Packs.Parser.ParseFrom(eventData.FixedData).SInitialObjects;
                    foreach (var objectInfo in InitData.ObjectInfos)
                    {
                        switch (objectInfo.SyncType)
                        {
                            case ObjectSyncType.PersonalOwn:
                                NetworkInstantiate(objectInfo);
                                break;
                            case ObjectSyncType.GroupOwn:
                                var obj = HeliosObjectList.Find(x =>
                                    x.ObjectInfo.ObjectID.ClientInstanceID == objectInfo.ObjectID.ClientInstanceID);
                                // obj.ObjectInfo = objectInfo;
                                obj.ObjectInfo.ObjectID.InstanceID = objectInfo.ObjectID.InstanceID;
                                obj.UpdateCustomData(objectInfo);
                                break;
                        }
                        
                    }
                    break;
                case EventCode.PKT_S_ADD_NETWORK_OBJECTS:
                    var data = Packs.Parser.ParseFrom(eventData.FixedData).SAddNetworkObjects;
                    foreach (var objectInfo in data.ObjectInfos)
                    {
                        if (objectInfo.SyncType == ObjectSyncType.GroupOwn)
                        {
                            var obj = HeliosObjectList.Find(x =>
                                x.ObjectInfo.ObjectID.ClientInstanceID ==
                                (int)objectInfo.ObjectID.ClientInstanceID);
                            obj.ObjectInfo.ObjectID.InstanceID = objectInfo.ObjectID.InstanceID;
                            // obj.hasInstanceId = true;
                        }
                        else
                        {
                            if(objectInfo.OwnerPlayerID != LocalPlayer.UserId)
                            {
                                NetworkInstantiate(objectInfo);
                            }
                            else
                            {
                                // RealtimeClient.MVSDebug(DebugLevel.INFO, objectInfo.ObjectID.ClientInstanceID.ToString());
                                var obj = HeliosObjectList.Find(x =>
                                    x.ObjectInfo.ObjectID.ClientInstanceID ==
                                    (int)objectInfo.ObjectID.ClientInstanceID);
                                obj.GetComponent<HeliosObject>().InstanceId = objectInfo.ObjectID.InstanceID;
                                // obj.GetComponent<HeliosObject>().hasInstanceId = true;
                                foreach (var heliosMonoBehavior in obj.GetComponentsInChildren<HeliosMonoBehavior>())
                                {
                                    heliosMonoBehavior.ObjectInfo.ObjectID.InstanceID = objectInfo.ObjectID.InstanceID;
                                    // heliosMonoBehavior.hasInstanceId = true;
                                }
                            }   
                        }
                    }
                    break;
                case EventCode.PKT_S_UPDATE_NETWORK_OBJECTS:
                    if(eventData.Sender == LocalPlayer.UserId) break;
                    var dataUpdate = Packs.Parser.ParseFrom(eventData.FixedData).SUpdateNetworkObjects;
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
                case (int)Protocol.EventCode.Rpc:
                    var dataRpc = Packs.Parser.ParseFrom(eventData.FixedData).CRpc;
                    var rpcObj = FindObjectById(dataRpc.ObjectID.InstanceID);
                    rpcObj.ExecuteRpc(dataRpc.MethodName, dataRpc.MethodArgs.ToByteArray());
                    break;
            }
        }

        private static void OnOperation(OperationResponse opRes)
        {
            switch (opRes.OperationCode)
            {
                case OperationCode.HEART_BEAT:
                    break;
                case OperationCode.ROOM_JOIN_OR_CREATE:
                    break;
                case OperationCode.GROUP_JOIN:
                    var dataGroupJoin = Packs.Parser.ParseFrom(opRes.FixedData).SGroupJoin;
                    if(dataGroupJoin.Result == Result.SuccessGroupCreate)
                    {
                        var pkt = new C_ADD_NETWORK_OBJECTS();
                        foreach (var obj in HeliosObjectList)
                        {
                            if(obj.ObjectInfo.SyncType == ObjectSyncType.PersonalOwn) continue;
                            obj.ObjectInfo.SyncType = ObjectSyncType.GroupOwn;
                            obj.ObjectInfo.OwnerPlayerID = 0;
                            pkt.ObjectInfos.Add(obj.ObjectInfo);
                        }
                        RaiseEvent(EventCode.PKT_C_ADD_NETWORK_OBJECTS, pkt);
                    }
                    break;
                case OperationCode.PLAYER_ID:
                    break;
            }
        }

        private static void OnClientStateChanged(ClientState arg1, ClientState arg2)
        {
            RealtimeClient.RealtimePeer.Listener.MVSDebug(DebugLevel.INFO, arg2.ToString());
            switch (arg2)
            {
                case ClientState.ConnectedToMVS:
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
            foreach (var obj in HeliosObjectList)
            {
                if(obj.GetComponent<HeliosObject>().IsMine)
                    NetworkRemoveObject(obj.ObjectInfo.ObjectID.InstanceID);
            }
        }
    }
}