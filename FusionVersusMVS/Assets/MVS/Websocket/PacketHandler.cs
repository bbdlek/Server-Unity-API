// C#용 templete

using System;
using System.Collections.Generic;
using System.Numerics;
using Google.Protobuf;
using MVS;
using MVS.Realtime;
using Protocol;
// using Enum;
using UnityEngine;
using WebSocketSharp;
using Random = UnityEngine.Random;
using Vector3 = Protocol.Vector3;

public partial class WebSocketHandler
{
    public enum PKT_ID
    {
        PKT_C_HEART_BEAT = 1000,
        PKT_S_HEART_BEAT = 1001,
        PKT_C_ROOM_JOIN_OR_CREATE = 1002,
        PKT_S_ROOM_JOIN_OR_CREATE = 1003,
        PKT_C_TEST_ROOM_LIST = 1004,
        PKT_S_TEST_ROOM_LIST = 1005,
        PKT_C_PLAYER_ID = 1006,
        PKT_S_PLAYER_ID = 1007,
        PKT_C_GROUP_LIST = 1008,
        PKT_S_GROUP_LIST = 1009,
        PKT_C_GROUP_JOIN = 1010,
        PKT_S_GROUP_JOIN = 1011,
        PKT_C_INITIAL_OBJECTS = 1012,
        PKT_S_INITIAL_OBJECTS = 1013,
        PKT_S_OTHER_CLIENT_JOINED = 1014,
        PKT_C_ADD_NETWORK_OBJECTS = 1015,
        PKT_S_ADD_NETWORK_OBJECTS = 1016,
        PKT_C_REMOVE_NETWORK_OBJECTS = 1017,
        PKT_S_REMOVE_NETWORK_OBJECTS = 1018,
        PKT_C_UPDATE_NETWORK_OBJECTS = 1019,
        PKT_S_UPDATE_NETWORK_OBJECTS = 1020,
        PKT_C_CHANGE_OBJECTS_OWNER = 1021,
        PKT_S_CHANGE_OBJECTS_OWNER = 1022,
        PKT_C_CHAT = 1023,
        PKT_S_CHAT = 1024,
    };

    /// <summary>
    /// -----    초기화 단계     ------
    /// </summary>
    public void Init()
    {
        handlerDic = new Dictionary<PKT_ID, Func<byte[], int, bool>>();
        handlerDic[PKT_ID.PKT_C_HEART_BEAT] = (bytes, len) => PacketHandler<C_HEART_BEAT>.Handling(Handle_C_HEART_BEAT, bytes, len);
        handlerDic[PKT_ID.PKT_S_HEART_BEAT] = (bytes, len) => PacketHandler<S_HEART_BEAT>.Handling(Handle_S_HEART_BEAT, bytes, len);
        handlerDic[PKT_ID.PKT_C_ROOM_JOIN_OR_CREATE] = (bytes, len) => PacketHandler<C_ROOM_JOIN_OR_CREATE>.Handling(Handle_C_ROOM_JOIN_OR_CREATE, bytes, len);
        handlerDic[PKT_ID.PKT_S_ROOM_JOIN_OR_CREATE] = (bytes, len) => PacketHandler<S_ROOM_JOIN_OR_CREATE>.Handling(Handle_S_ROOM_JOIN_OR_CREATE, bytes, len);
        handlerDic[PKT_ID.PKT_C_TEST_ROOM_LIST] = (bytes, len) => PacketHandler<C_TEST_ROOM_LIST>.Handling(Handle_C_TEST_ROOM_LIST, bytes, len);
        handlerDic[PKT_ID.PKT_S_TEST_ROOM_LIST] = (bytes, len) => PacketHandler<S_TEST_ROOM_LIST>.Handling(Handle_S_TEST_ROOM_LIST, bytes, len);
        handlerDic[PKT_ID.PKT_C_PLAYER_ID] = (bytes, len) => PacketHandler<C_PLAYER_ID>.Handling(Handle_C_PLAYER_ID, bytes, len);
        handlerDic[PKT_ID.PKT_S_PLAYER_ID] = (bytes, len) => PacketHandler<S_PLAYER_ID>.Handling(Handle_S_PLAYER_ID, bytes, len);
        handlerDic[PKT_ID.PKT_C_GROUP_LIST] = (bytes, len) => PacketHandler<C_GROUP_LIST>.Handling(Handle_C_GROUP_LIST, bytes, len);
        handlerDic[PKT_ID.PKT_S_GROUP_LIST] = (bytes, len) => PacketHandler<S_GROUP_LIST>.Handling(Handle_S_GROUP_LIST, bytes, len);
        handlerDic[PKT_ID.PKT_C_GROUP_JOIN] = (bytes, len) => PacketHandler<C_GROUP_JOIN>.Handling(Handle_C_GROUP_JOIN, bytes, len);
        handlerDic[PKT_ID.PKT_S_GROUP_JOIN] = (bytes, len) => PacketHandler<S_GROUP_JOIN>.Handling(Handle_S_GROUP_JOIN, bytes, len);
        handlerDic[PKT_ID.PKT_C_INITIAL_OBJECTS] = (bytes, len) => PacketHandler<C_INITIAL_OBJECTS>.Handling(Handle_C_INITIAL_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_S_INITIAL_OBJECTS] = (bytes, len) => PacketHandler<S_INITIAL_OBJECTS>.Handling(Handle_S_INITIAL_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_S_OTHER_CLIENT_JOINED] = (bytes, len) => PacketHandler<S_OTHER_CLIENT_JOINED>.Handling(Handle_S_OTHER_CLIENT_JOINED, bytes, len);
        handlerDic[PKT_ID.PKT_C_ADD_NETWORK_OBJECTS] = (bytes, len) => PacketHandler<C_ADD_NETWORK_OBJECTS>.Handling(Handle_C_ADD_NETWORK_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_S_ADD_NETWORK_OBJECTS] = (bytes, len) => PacketHandler<S_ADD_NETWORK_OBJECTS>.Handling(Handle_S_ADD_NETWORK_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_C_REMOVE_NETWORK_OBJECTS] = (bytes, len) => PacketHandler<C_REMOVE_NETWORK_OBJECTS>.Handling(Handle_C_REMOVE_NETWORK_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_S_REMOVE_NETWORK_OBJECTS] = (bytes, len) => PacketHandler<S_REMOVE_NETWORK_OBJECTS>.Handling(Handle_S_REMOVE_NETWORK_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_C_UPDATE_NETWORK_OBJECTS] = (bytes, len) => PacketHandler<C_UPDATE_NETWORK_OBJECTS>.Handling(Handle_C_UPDATE_NETWORK_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_S_UPDATE_NETWORK_OBJECTS] = (bytes, len) => PacketHandler<S_UPDATE_NETWORK_OBJECTS>.Handling(Handle_S_UPDATE_NETWORK_OBJECTS, bytes, len);
        handlerDic[PKT_ID.PKT_C_CHANGE_OBJECTS_OWNER] = (bytes, len) => PacketHandler<C_CHANGE_OBJECTS_OWNER>.Handling(Handle_C_CHANGE_OBJECTS_OWNER, bytes, len);
        handlerDic[PKT_ID.PKT_S_CHANGE_OBJECTS_OWNER] = (bytes, len) => PacketHandler<S_CHANGE_OBJECTS_OWNER>.Handling(Handle_S_CHANGE_OBJECTS_OWNER, bytes, len);
        handlerDic[PKT_ID.PKT_C_CHAT] = (bytes, len) => PacketHandler<C_CHAT>.Handling(Handle_C_CHAT, bytes, len);
        handlerDic[PKT_ID.PKT_S_CHAT] = (bytes, len) => PacketHandler<S_CHAT>.Handling(Handle_S_CHAT, bytes, len);
    }

#region RECV_Functions
    
    bool Handle_S_HEART_BEAT(byte[] data)
    {
        var packet = S_HEART_BEAT.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        // GameCore.Instance.ping.SetPing();
        return true;
    }
    
    bool Handle_S_ROOM_JOIN_OR_CREATE(byte[] data)
    {
        var packet = S_ROOM_JOIN_OR_CREATE.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Listener.MVSDebug(DebugLevel.ERROR, $"packet result : {packet.Result}");
            return false;
        }
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_ROOM_JOIN_OR_CREATE,
            Sender = 0
        };
        Listener.OnEvent(eventData);
        
        OperationResponse res = new OperationResponse
        {
            OperationCode = OperationCode.JoinRoom,
            ReturnCode = 0,
            Data = data
        };
        Listener.OnOperationResponse(res);
        
        // if (
        //     ConsistencyCheck(MVSRunner.Instance.appID, packet.AppID,"AppID") &&
        //     ConsistencyCheck(MVSRunner.Instance.waplRoomID, packet.WaplRoomID,"WaplRoomID") &&
        //     ConsistencyCheck(MVSRunner.Instance.name, packet.Name,"Name")
        // )
        // {
        //     // MVSRunner.isRoomJoined = true;
        //     // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_GROUP_JOIN);
        //     // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_GROUP_LIST);
        //     return true;
        // }

        return true;
    }

    bool Handle_S_TEST_ROOM_LIST(byte[] data)
    {
        var packet = S_TEST_ROOM_LIST.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }

        Log(packet.RoomInfos);
        // if (GlobalCore.Instance.testUser.checkRoomCreateInfo)
        // {
        //     GlobalCore.Instance.testUser.checkRoomCreateInfo = false;
        //
        //     bool result = false;
        //     foreach (var roomInfo in packet.RoomInfos)
        //     {
        //         if (roomInfo.RoomID == GlobalCore.Instance.testUser.waplRoomID)
        //         {
        //             if (
        //                 ConsistencyCheck(GlobalCore.Instance.testUser.appID, roomInfo.AppID, "AppID") &&
        //                 ConsistencyCheck(GlobalCore.Instance.testUser.roomName, roomInfo.Name, "Name")
        //             )
        //             {
        //                 result = true;
        //                 break;
        //             }
        //         }
        //     }
        //     
        //     return result;
        // }
        
        return true;
    }
    
    bool Handle_S_PLAYER_ID(byte[] data)
    {
        var packet = S_PLAYER_ID.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_PLAYER_ID,
            Sender = 0,
            Data = data
        };
        Listener.OnEvent(eventData);

        // GlobalCore.Instance.testUser.playerIDs.TryAdd(clientNum, packet.PlayerID);
        // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_ADD_NETWORK_OBJECTS);

        return true;
    }
    
    bool Handle_S_GROUP_LIST(byte[] data)
    {
        
        var packet = S_GROUP_LIST.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }

        foreach (var group in packet.GroupInfos)
        {
            Log(group.GroupID);
        }

        return true;
    }
    
    bool Handle_S_GROUP_JOIN(byte[] data)
    {
        
        var packet = S_GROUP_JOIN.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }

        // if (ConsistencyCheck(MVSRunner.Instance.sceneNumber, packet.GroupInfo.GroupID.SceneNumber, "SceneNumber") &&
        //     ConsistencyCheck(MVSRunner.Instance.channelID, packet.GroupInfo.GroupID.ChannelID, "ChannelID"))
        // {
        //     // Log(packet.GroupInfo);
        //     // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_PLAYER_ID);
        //     // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_INITIAL_OBJECTS);
        //     // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_CHAT);
        //     return true;
        // }
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_GROUP_JOIN,
            Sender = 0
        };
        Listener.OnEvent(eventData);
        
        OperationResponse res = new OperationResponse
        {
            OperationCode = OperationCode.JoinGroup,
            ReturnCode = 0,
            Data = data
        };
        Listener.OnOperationResponse(res);

        return true;
    }
    
    bool Handle_S_INITIAL_OBJECTS(byte[] data)
    {
        var packet = S_INITIAL_OBJECTS.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }

        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_INITIAL_OBJECTS,
            Sender = 0,
            Data = data
        };
        Listener.OnEvent(eventData);

        return true;
    }
    
    bool Handle_S_OTHER_CLIENT_JOINED(byte[] data)
    {
        var packet = S_OTHER_CLIENT_JOINED.Parser.ParseFrom(data);

        // if (ConsistencyCheck(GlobalCore.Instance.testUser.sceneNumber, packet.GroupID.SceneNumber, "SceneNumber") &&
        //     ConsistencyCheck(GlobalCore.Instance.testUser.channelID, packet.GroupID.ChannelID, "ChannelID"))
        // {
        //     Log(packet);
        //     return true;
        // }
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_OTHER_CLIENT_JOINED,
            Sender = 0,
            Data = data
        };
        Listener.OnEvent(eventData);
        
        return true;
    }
    
    bool Handle_S_ADD_NETWORK_OBJECTS(byte[] data)
    {
        var packet = S_ADD_NETWORK_OBJECTS.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }

        // GlobalCore.Instance.testUser.AddObjectInfo(packet.ObjectInfos);
        Log(packet.ObjectInfos);
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_ADD_NETWORK_OBJECTS,
            Sender = 0,
            Data = data
        };
        Listener.OnEvent(eventData);
        
        // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_UPDATE_NETWORK_OBJECTS);
        // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_CHANGE_OBJECTS_OWNER);
        // ButtonManager.SetButtonEnable(PKT_ID.PKT_C_REMOVE_NETWORK_OBJECTS);

        return true;
    }
    
    bool Handle_S_REMOVE_NETWORK_OBJECTS(byte[] data)
    {
        
        var packet = S_REMOVE_NETWORK_OBJECTS.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_REMOVE_NETWORK_OBJECTS,
            Sender = 0,
            Data = data
        };
        Listener.OnEvent(eventData);

        return true;
    }
    
    bool Handle_S_UPDATE_NETWORK_OBJECTS(byte[] data)
    {
        
        var packet = S_UPDATE_NETWORK_OBJECTS.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        
        EventData eventData = new EventData
        {
            code = EventCode.PKT_S_UPDATE_NETWORK_OBJECTS,
            Sender = 0,
            Data = data
        };
        Listener.OnEvent(eventData);
        
        // GlobalCore.Instance.testUser.UpdateObjectInfo(packet.ObjectInfos);
        Log(packet.ObjectInfos);

        return true;
    }
    
    bool Handle_S_CHANGE_OBJECTS_OWNER(byte[] data)
    {
        
        var packet = S_CHANGE_OBJECTS_OWNER.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        
        // GlobalCore.Instance.testUser.UpdateObjectInfo(packet.ObjectInfos);
        Log(packet);

        return true;
    }
    
    bool Handle_S_CHAT(byte[] data)
    {
        
        var packet = S_CHAT.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        
        // Log($"{packet.PlayerInfo.Name}:{packet.Msg}");
        Debug.Log($"{packet.PlayerInfo.Name}:{packet.Msg}");
        return true;
    }

#endregion RECV_Functions
    
#region REQ_Functions
    bool Handle_C_HEART_BEAT(byte[] data)
    {
        var packet = C_HEART_BEAT.Parser.ParseFrom(data);

        // GameCore.Instance.ping.SendPing();
        SendData(PKT_ID.PKT_C_HEART_BEAT, packet.ToByteArray());

        return true;
    }
    bool Handle_C_ROOM_JOIN_OR_CREATE(byte[] data)
    {
        var packet = C_ROOM_JOIN_OR_CREATE.Parser.ParseFrom(data);
        // packet.AuthToken = GlobalCore.Instance.testUser.authToken;
        // if (isMain)
        // {
        //     //GlobalCore.Instance.testUser.waplRoomID = packet.WaplRoomID = (ulong)Random.Range(1, Int64.MaxValue);
        //     //GlobalCore.Instance.testUser.appID = packet.AppID = (ulong)Random.Range(1, Int64.MaxValue);
        //     GlobalCore.Instance.testUser.waplRoomID = packet.WaplRoomID = (ulong)1;
        //     GlobalCore.Instance.testUser.appID = packet.AppID = (ulong)1;
        //     GlobalCore.Instance.testUser.roomName = packet.Name = "UnitTest";   
        // }
        // else
        // {
        //     packet.WaplRoomID = GlobalCore.Instance.testUser.waplRoomID;
        //     packet.AppID = GlobalCore.Instance.testUser.appID;
        //     packet.Name = GlobalCore.Instance.testUser.roomName;
        // }
        SendData(PKT_ID.PKT_C_ROOM_JOIN_OR_CREATE, packet.ToByteArray());

        return true;
    }
    bool Handle_C_TEST_ROOM_LIST(byte[] data)
    {
        var packet = C_TEST_ROOM_LIST.Parser.ParseFrom(data);
        
        SendData(PKT_ID.PKT_C_TEST_ROOM_LIST, packet.ToByteArray());

        return true;
    }
    bool Handle_C_PLAYER_ID(byte[] data)
    {
        var packet = C_PLAYER_ID.Parser.ParseFrom(data);
        SendData(PKT_ID.PKT_C_PLAYER_ID, packet.ToByteArray());

        return true;
    }
    bool Handle_C_GROUP_LIST(byte[] data)
    {
        var packet = C_GROUP_LIST.Parser.ParseFrom(data);

        SendData(PKT_ID.PKT_C_GROUP_LIST, packet.ToByteArray());

        return true;
    }
    bool Handle_C_GROUP_JOIN(byte[] data)
    {
        var packet = C_GROUP_JOIN.Parser.ParseFrom(data);
        // packet.GroupID = new GroupID();
        // packet.GroupID.SceneNumber = GlobalCore.Instance.testUser.sceneNumber;
        // packet.GroupID.ChannelID = GlobalCore.Instance.testUser.channelID;
        SendData(PKT_ID.PKT_C_GROUP_JOIN, packet.ToByteArray());

        return true;
    }
    bool Handle_C_INITIAL_OBJECTS(byte[] data)
    {
        var packet = C_INITIAL_OBJECTS.Parser.ParseFrom(data);

        SendData(PKT_ID.PKT_C_INITIAL_OBJECTS, packet.ToByteArray());

        return true;
    }
    bool Handle_C_ADD_NETWORK_OBJECTS(byte[] data)
    {
        var packet = C_ADD_NETWORK_OBJECTS.Parser.ParseFrom(data);
        SendData(PKT_ID.PKT_C_ADD_NETWORK_OBJECTS, packet.ToByteArray());

        return true;
    }
    bool Handle_C_REMOVE_NETWORK_OBJECTS(byte[] data)
    {
        var packet = C_REMOVE_NETWORK_OBJECTS.Parser.ParseFrom(data);
        // foreach (var objectInfo in GlobalCore.Instance.testUser.ObjectInfos.Values)
        // {
        //     if (objectInfo.OwnerPlayerID == GlobalCore.Instance.testUser.playerIDs[clientNum])
        //     {
        //         packet.ObjectInfos.Add(objectInfo);
        //         break;                
        //     }
        // }
        SendData(PKT_ID.PKT_C_REMOVE_NETWORK_OBJECTS, packet.ToByteArray());

        return true;
    }
    bool Handle_C_UPDATE_NETWORK_OBJECTS(byte[] data)
    {
        var packet = C_UPDATE_NETWORK_OBJECTS.Parser.ParseFrom(data);
        
        // foreach (var objectInfo in GlobalCore.Instance.testUser.ObjectInfos.Values)
        // {
        //     if (objectInfo.OwnerPlayerID == GlobalCore.Instance.testUser.playerIDs[clientNum])
        //     {
        //         var vector = new Vector3();
        //         vector.X = Random.Range(-7.7f, 7.7f);
        //         vector.Y = Random.Range(-4.4f, 4.4f);
        //         vector.Z = Random.Range(-9f, 9f);
        //         CustomNumberProp position = new CustomNumberProp{ Index = PropsID.Position3D };
        //         position.Value.Add(vector.X);
        //         position.Value.Add(vector.Y);
        //         position.Value.Add(vector.Z);
        //         objectInfo.NumberProps[(int)PropsID.Position3D] = position;
        //         packet.ObjectInfos.Add(objectInfo);
        //     }
        // }

        SendData(PKT_ID.PKT_C_UPDATE_NETWORK_OBJECTS, packet.ToByteArray());

        return true;
    }
    bool Handle_C_CHANGE_OBJECTS_OWNER(byte[] data)
    {
        var packet = C_CHANGE_OBJECTS_OWNER.Parser.ParseFrom(data);
        // foreach (var objectInfo in GlobalCore.Instance.testUser.ObjectInfos.Values)
        // {
        //     if (objectInfo.OwnerPlayerID == GlobalCore.Instance.testUser.playerIDs[clientNum])
        //     {
        //         packet.ObjectInfos.Add(objectInfo);
        //         break;
        //     }
        // }

        packet.NewOwnerPlayerID = 0; 
        SendData(PKT_ID.PKT_C_CHANGE_OBJECTS_OWNER, packet.ToByteArray());

        return true;
    }
    bool Handle_C_CHAT(byte[] data)
    {
        var packet = C_CHAT.Parser.ParseFrom(data);
        // packet.Msg = "UnitTest Chat Test";
        SendData(PKT_ID.PKT_C_CHAT, packet.ToByteArray());

        return true;
    }
    
#endregion REQ_Functions
}