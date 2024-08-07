using System;
using System.Collections;
using System.Collections.Generic;
using _1_Scripts._8_HeliosTest;
using MVS.Helios;
using MVS.Realtime;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TestGameManager : HeliosMonoBehavior, IConnectionCallbacks
{
    [SerializeField]
    private TestDefault _testDefault;

    private void OnEnable()
    {
        HeliosNetwork.AddCallbackTarget(this);
    }
    
    private void OnDisable()
    {
        HeliosNetwork.RemoveCallbackTarget(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // RPC(nameof(Test));
            HeliosNetwork.Disconnect();
        }
    }

    [HeliosRPC]    
    private void Test()
    {
        _testDefault.Test();
    }

    public void OnConnectedToMasterServer()
    {
        
    }

    public void OnConnected()
    {
        
    }

    public void OnDisconnected()
    {
        Debug.Log("Hello");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        
    }
}
