using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVS.Realtime;
using UnityEngine;

public class TestClient : RealtimeClient
{
    public override void OnStatusChanged(StatusCode statusCode)
    {
        base.OnStatusChanged(statusCode);
    }

    public override void MVSDebug(DebugLevel debugLevel, string msg)
    {
        base.MVSDebug(debugLevel, msg);
        switch (debugLevel)
        {
            case DebugLevel.INFO:
                Debug.Log(msg);
                break;
            case DebugLevel.WARNING:
                Debug.LogWarning(msg);
                break;
            case DebugLevel.ERROR:
                Debug.LogError(msg);
                break;
        }
    }

    public override void OnOperationResponse(OperationResponse operationResponse)
    {
        base.OnOperationResponse(operationResponse);
        switch (operationResponse.OperationCode)
        {
            case OperationCode.ROOM_JOIN_OR_CREATE:
                Debug.Log(CurrentRoom.RoomInfo.Name);
                break;
        }
    }
}
