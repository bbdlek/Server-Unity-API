using System;
using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviorHeliosCallbacks
{
    public GameObject[] prefabsForSpawn;

    [Networked] public int score2;
    
    private void Awake()
    {
        HeliosNetwork.SendRate = 1;
    }

    [HeliosRPC(target: "ALL")]
    public void AddScore()
    {
        score2++;
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
            RPC("AddScore");
            // AddScore();
        }
    }
}
