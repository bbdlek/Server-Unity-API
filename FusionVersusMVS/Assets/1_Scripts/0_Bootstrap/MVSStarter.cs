using System.Collections;
using System.Collections.Generic;
using MVS;
using Protocol;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MVSStarter : MonoBehaviour, MVSRunnerCallbacks
{
    private MVSRunner _runner;

    public async void ConnectToMVS()
    {
        _runner = gameObject.GetOrAddComponent<MVSRunner>();
        await _runner.ConnectToMVS();
    }

    public async void StartGame()
    {
        await _runner.GameStart();
    }

    public void SendChatTest()
    {
        _runner.SendChat();
    }

    public void OnRoomJoined(MVSRunner runner)
    {
        Debug.Log("OnRoomJoined");
    }

    public void OnGroupJoined(MVSRunner runner)
    {
        
    }

    public void OnPlayerJoined(MVSRunner runner, PlayerInfo playerInfo)
    {
        Debug.Log("OnPlayerJoined" + playerInfo.Name);
        Vector3 spawnPosition = Vector3.zero;
        // MVSNetworkObject networkPlayerObject = runner.Spawn()
    }

    public void OnPlayerLeft(MVSRunner runner, PlayerInfo playerInfo)
    {
        
    }

    public void OnInput(MVSRunner runner)
    {
        
    }

    public void OnShutDown(MVSRunner runner)
    {
        
    }

    public void OnConnectedToServer(MVSRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnGroupListUpdated(MVSRunner runner)
    {
        
    }

    public void OnDataReceived(MVSRunner runner)
    {
        
    }
}
