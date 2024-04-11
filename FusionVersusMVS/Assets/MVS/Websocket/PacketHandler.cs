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
        PKT_C_OPERATION = 1000,
        PKT_S_OPERATION = 1001,
        PKT_S_EVENT = 1002,
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
        opData.CustomData = packet.CustomData;
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
        eventData.CustomData = packet.CustomData;
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