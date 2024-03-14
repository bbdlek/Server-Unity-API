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
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using OperationCode = MVS.Realtime.OperationCode;
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
        PKT_C_EVENT = 1025,
        PKT_C_OPERATION = 1026,
        PKT_S_OPERATION = 1027,
        PKT_S_EVENT = 1028,
    };

    /// <summary>
    /// -----    초기화 단계     ------
    /// </summary>
    public void Init()
    {
        handlerDic = new Dictionary<PKT_ID, Func<byte[], int, bool>>();
        handlerDic[PKT_ID.PKT_C_OPERATION] = (bytes, len) => PacketHandler<C_OPERATION>.Handling(Handle_C_OPERATION, bytes, len);
        handlerDic[PKT_ID.PKT_S_OPERATION] = (bytes, len) => PacketHandler<S_OPERATION>.Handling(Handle_S_OPERATION, bytes, len);
        handlerDic[PKT_ID.PKT_S_EVENT] = (bytes, len) => PacketHandler<S_EVENT>.Handling(Handle_S_EVENT, bytes, len);
    }

#region RECV_Functions
    bool Handle_S_OPERATION(byte[] data)
    {
        var packet = S_OPERATION.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }

        OperationResponse opData = new OperationResponse
        {
            OperationCode = (OperationCode)packet.OperationCode,
            ReturnCode = 0,
            FixedData = packet.FixedData.ToByteArray()
        };
        opData.CustomData = new List<byte[]>();
        foreach (var customStruct in packet.CustomData)
        {
            opData.CustomData.Add(customStruct.ToByteArray());
        }
        Listener.OnOperationResponse(opData);

        return true;
    }

    bool Handle_S_EVENT(byte[] data)
    {
        var packet = S_EVENT.Parser.ParseFrom(data);
        if (packet.Result != Result.Success)
        {
            Debug.LogError($"packet result : {packet.Result}");
            return false;
        }
        EventData eventData = new EventData
        {
            code = packet.EventCode,
            Sender = packet.Sender.PlayerID,
            FixedData = packet.FixedData.ToByteArray()
        };
        eventData.CustomData = new List<byte[]>();
        foreach (var customStruct in packet.CustomData)
        {
            eventData.CustomData.Add(customStruct.ToByteArray());
        }
        Listener.OnEvent(eventData);

        return true;
    }

#endregion RECV_Functions
    
#region REQ_Functions
    bool Handle_C_OPERATION(byte[] data)
    {
        var pakcet = C_OPERATION.Parser.ParseFrom(data);

        SendData(PKT_ID.PKT_C_OPERATION, pakcet.ToByteArray());

        return true;
    }
    
#endregion REQ_Functions
}