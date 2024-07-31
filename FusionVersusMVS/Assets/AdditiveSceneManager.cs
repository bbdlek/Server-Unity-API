using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using Vector3 = UnityEngine.Vector3;

public class AdditiveSceneManager : MonoBehaviorHeliosCallbacks
{
    public GameObject[] prefabsForSpawn;

    public override void Awake()
    {
        Debug.Log(HeliosNetwork.CurrentGroup.GroupInfo.GroupID.SceneNumber);
        base.Awake();
        Debug.Log("Awake");
    }
    
    private void Start()
    {
        Debug.Log("Start");
        GameObject cubeObject = HeliosNetwork.Instantiate(prefabsForSpawn[Random.Range(0, prefabsForSpawn.Length)], Vector3.zero, Quaternion.identity);
    }

    public override void OnConnectedToMasterServer()
    {
        
    }

    public override void OnConnected()
    {
        
    }

    public override void OnDisconnected()
    {
        
    }

    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        
    }

    public override void OnCreatedRoom()
    {
        
    }

    public override void OnCreatedRoomFailed(string message)
    {
        
    }

    public override void OnJoinedRoom()
    {
        
    }

    public override void OnJoinedRoomFailed(string message)
    {
        
    }

    public override void OnLeftRoom()
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
    }

    public override void OnPlayerLeftGroup(Player otherPlayer)
    {
        
    }

    public override void OnEvent(EventData eventData)
    {
        switch (eventData.code)
        {
            case EventCode.PKT_S_INITIAL_OBJECTS:
                var initData = Packs.Parser.ParseFrom(eventData.FixedData).SInitialObjects;
                Debug.Log(initData.ObjectInfos.Count);
                foreach (var objectInfo in initData.ObjectInfos)
                {
                    switch (objectInfo.SyncType)
                    {
                        case ObjectSyncType.PersonalOwn:
                            break;
                        case ObjectSyncType.GroupOwn:
                            break;
                    }
                        
                }
                break;
        }
    }

    public override void OnErrorInfo(string errorInfo)
    {
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        
    }

    public override void OnObjectInstantiated(ObjectInfo objectInfo)
    {
        
    }

    public override void OnObjectDestroyed(ObjectInfo objectInfo)
    {
        
    }

    public override void OnCreatedGroup()
    {
        
    }

    public override void OnCreatedGroupFailed(string message)
    {
        
    }

    public override void OnJoinedGroup()
    {
        
    }

    public override void OnJoinedGroupFailed(string message)
    {
        
    }

    public override void OnLeftGroup()
    {
        
    }

    public override void OnPlayerEnteredGroup(Player newPlayer)
    {
        
    }
}
