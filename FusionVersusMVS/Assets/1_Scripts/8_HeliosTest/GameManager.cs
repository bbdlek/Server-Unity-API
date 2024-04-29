using System;
using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using QFSW.QC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviorHeliosCallbacks
{
    public GameObject[] prefabsForSpawn;

    public int score2;
    public int score3;
    [HNSync] public int test;

    [HNSync] public Vector2 testVec2 = new Vector2(2, 3);

    [HeliosRPC(target: "ALL")]
    public void AddScore(int added1, int added2)
    {
        score2 += added1;
        score3 += added2;
    }

    [HeliosRPC("All")]
    public void ChangeVec2()
    {
        testVec2.x++;
    }

    public void AddTest()
    {
        test++; 
    }

    [Command]
    public void ConnectCustom(string url, int port)
    {
        HeliosNetwork.HeliosSettings.AppSettings.Server = url;
        HeliosNetwork.HeliosSettings.AppSettings.Port = port;
        HeliosNetwork.ConnectUsingSettings();
    }

    public void OnClickConnectBtn()
    {
        HeliosNetwork.ConnectUsingSettings();
    }

    public void OnClickJoinRoomBtn()
    {
        HeliosNetwork.JoinOrCreateRoom("auth", 1, 1, "NewRoom");
    }
    
    public void OnClickJoinGroupBtn()
    {
        HeliosNetwork.JoinGroup(1, 1);
    }

    public void OnClickCreateObjectBrn()
    {
        GameObject cubeObject = HeliosNetwork.Instantiate(prefabsForSpawn[Random.Range(0, prefabsForSpawn.Length)], Vector3.zero, Quaternion.identity);
    }

    public async void OnClickRoomTaskBtn()
    {
        await HeliosNetwork.GetRoomList();
        foreach (var room in HeliosNetwork.RoomList)
        {
            Debug.Log(room.RoomInfo.Name);
        }
    }
    
    public async void OnClickGroupTaskBtn()
    {
        await HeliosNetwork.GetGroupList();
        foreach (var group in HeliosNetwork.GroupList)
        {
            Debug.Log(group.GroupInfo.GroupID);
        }
    }

    public void OnDropDown(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                HeliosNetwork.HeliosSettings.AppSettings.Protocol = ConnectionProtocol.WebSocket;
                HeliosNetwork.HeliosSettings.AppSettings.Port = 30080;
                Debug.Log($"Changed To Websocket {HeliosNetwork.HeliosSettings.AppSettings.Port}");
                break;
            case 1:
                HeliosNetwork.HeliosSettings.AppSettings.Protocol = ConnectionProtocol.Tcp;
                HeliosNetwork.HeliosSettings.AppSettings.Port = 30082;
                Debug.Log($"Changed To Tcp {HeliosNetwork.HeliosSettings.AppSettings.Port}");
                break;
        }
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName);
    }

    public override void OnPlayerEnteredGroup(Player newPlayer)
    {
        base.OnPlayerEnteredGroup(newPlayer);
        Debug.Log(newPlayer.NickName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RPC("AddScore", 3, 6);
            RPC("ChangeVec2");
            // AddScore();
            AddTest();
        }
    }
}
