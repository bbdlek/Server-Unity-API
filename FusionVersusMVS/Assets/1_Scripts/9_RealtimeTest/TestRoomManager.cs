using System.Collections;
using System.Collections.Generic;
using MVS.Realtime;
using UnityEngine;

public class TestRoomManager : Singleton<TestRoomManager>, IMakingRoomCallbacks, IConnectionCallbacks
{
    public void OnClickRoomJoinBtn()
    {
        TestManager.Instance.Client.OpCreateRoom("", 0, 0, "RoomName");
    }

    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public void OnCreatedRoomFailed(short failCode, string message)
    {
        
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }

    public void OnJoinedRoomFailed(short failCode, string message)
    {
        
    }

    public void OnLeftRoom()
    {
        
    }

    public void OnConnected()
    {
        Debug.Log("OnConnected");
    }

    public void OnConnectedToMaster()
    {
        
    }

    public void OnDisconnected()
    {
        
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        
    }
}
