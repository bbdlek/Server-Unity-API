using System;
using MVS.Realtime;
using UnityEngine;

public class TestManager : Singleton<TestManager>
{
    public TestClient Client;

    private AppSettings _appSettings;
    
    void Awake()
    {
        _appSettings = new AppSettings
        {
            AppId = "AppID",
            AppVersion = "AppVersion",
            FixedRegion = "FixedRegion",
            Server = "222.122.186.49",
            Port = 80,
            IsUsingNameServer = false,
            Protocol = ConnectionProtocol.WebSocket,
            DebugLevel = DebugLevel.ALL
        };
        
        Client = new TestClient
        {
            AppVersion = null,
            AppId = null,
        };
    }

    private void OnEnable()
    {
        if (Client.IsConnected)
        {
            Debug.Log("Already Connected");
        }
        Client.StateChanged += OnStateChanged;
        
        Client.AddCallbackTarget(TestRoomManager.Instance);
        Client.AddCallbackTarget(TestGroupManager.Instance);
    }

    private void OnStateChanged(ClientState fromState, ClientState toState)
    {
        Debug.Log($"FromState: {fromState} toState: {toState}");
        switch (toState)
        {
            case ClientState.Created:
                break;
        }
    }

    public void OnClickLoginBtn()
    {
        Client.Connect("222.122.186.49", "80", "AppId", ServerConnection.MVS);
    }

    public void OnClickAddNetworkObjectBtn()
    {
        Client.OpAddNetworkObject();
    }

    // private void Update()
    // {
    //     Client.Service();
    // }
}
