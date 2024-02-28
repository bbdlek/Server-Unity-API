using System;
using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using UnityEngine;

public class GameManager : MonoBehaviorHeliosCallbacks
{
    private void Awake()
    {
        HeliosNetwork.SendRate = 30;
        HeliosNetwork.PrefabPool = new DefaultPrefabPool();
    }

    private void Update()
    {
        if(HeliosNetwork.IsConnected)
            HeliosNetwork.Service();
    }

    public void OnClickConnectBtn()
    {
        HeliosNetwork.ConnectUsingSettings();
    }

    public void OnClickJoinRoomBtn()
    {
        HeliosNetwork.JoinOrCreateRoom("auth", 0, 1, "NewRoom");
    }
    
    public void OnClickJoinGroupBtn()
    {
        HeliosNetwork.JoinGroup(1, 1);
    }

    public void OnClickCreateObjectBrn()
    {
        GameObject cubeObject = HeliosNetwork.Instantiate("Cube", Vector3.zero, Quaternion.identity);
    }

    public override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("OnConnected");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("JoinedRoom");
    }
}
