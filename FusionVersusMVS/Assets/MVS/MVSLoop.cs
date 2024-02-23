using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MVSLoop : MVSNetworkCallbacks
{
    private MVSNetworkCallbacks _callbacks;
    
    internal unsafe MVSLoop(MVSLoopArgs args)
    {
        _callbacks = args.Callbacks;
    }

    public void OnConnected()
    {
        MVSNetworkCallbacks.OnConnected();
    }

    public void OnConnectedToMaster()
    {
        
    }

    public void OnDisconnected()
    {
        
    }
}
