using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Bayou;
using FishNet.Transporting.Multipass;
using UnityEngine;
using FishNet.Transporting.Tugboat;

public class ConnectionStarter : MonoBehaviour
{
    private Multipass _mp;

    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }
    
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopping)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    private void Start() 
    {
        _mp = InstanceFinder.TransportManager.GetTransport<Multipass>();
        #if UNITY_WEBGL && !UNITY_EDITOR
        _mp.SetClientTransport<Bayou>();
        #else
        _mp.SetClientTransport<Tugboat>();
        #endif
        
        if(ParrelSync.ClonesManager.IsClone())
        {
            _mp.StartConnection(false);
        }
        else
        {
            _mp.StartConnection(true);
            _mp.StartConnection(false);
        }
    }
}
