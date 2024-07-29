using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MVS.Helios;
using MVS.Realtime;
using Protocol;
using QFSW.QC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class Costume
{
    public int Body;
    public int BodyPart;
    public int Eyes;
    public int Gloves;
    public int HeadPart;
    public int Mouth;
    public int Tail;

    public static Costume Default()
    {
        return new Costume
        {
            Body = 1,
            BodyPart = 0,
            Eyes = 1,
            Gloves = 0,
            HeadPart = 0,
            Mouth = 1,
            Tail = 0
        };
    }
    
    public static Costume DeepCopy(Costume original)
    {
        return new Costume
        {
            Body = original.Body,
            BodyPart = original.BodyPart,
            Eyes = original.Eyes,
            Gloves = original.Gloves,
            HeadPart = original.HeadPart,
            Mouth = original.Mouth,
            Tail = original.Tail
        };
    }
}

public class GameManager : MonoBehaviorHeliosCallbacks
{
    public GameObject[] prefabsForSpawn;

    public int score2;
    public int score3;
    [HNSync, OnChanged(nameof(OnChangedVec))] public int test;

    [HNSync] public Vector2 testVec2 = new Vector2(2, 3);

    private void OnChangedVec()
    {
        Debug.Log("OnChangedVec");
    }

    [HNSync] public List<int> testList = new List<int>(){1, 2, 3};

    public Costume costume = default;
    
    // [HNSync] public Color color = new Color();
    // [HNSync] public Color32 color32 = new Color32();

    [HNSync] public Dictionary<int, string> testDic = new Dictionary<int, string>();

    public TMP_Text Txt_UserId;
    public TMP_Text Txt_IsMaster;

    public enum EnumTest
    {
        test1, test2
    }

    [HNSync] public EnumTest enumTest = EnumTest.test1;

    [HeliosRPC]
    public void AddScore(int added1, int added2)
    {
        score2 += added1;
        score3 += added2;
    }
    
    [HeliosRPC]
    private void ChangeCostume(Costume ct)
    {
        Debug.Log(ct);
    }

    public GameObject testCube;
    
    [HeliosRPC]
    private void PendulumRPC(float max = 1f, float min = 2f)
    {
        Debug.Log($"max : {max}, min : {min}");
    }

    [HeliosRPC]
    public void ChangeVec2()
    {
        testVec2.x++;
        if(testDic.Count > 0)
            Debug.Log(testDic[0]);
    }

    public void AddTest()
    {
        test++; 
        testList.Add(10);
        testDic[0] = test.ToString();
        enumTest = EnumTest.test2;
    }

    [Command]
    public void ConnectCustom(string url, int port)
    {
        HeliosNetwork.HeliosSettings.AppSettings.Server = url;
        HeliosNetwork.HeliosSettings.AppSettings.Port = port;
        // HeliosNetwork.ConnectUsingSettings();
    }

    [Command]
    public void SetAppKey(string appKey)
    {
        HeliosNetwork.HeliosSettings.AppSettings.AppId = appKey;
    }

    private void Start()
    {
        Debug.Log(HeliosNetwork.HeliosSettings.AppSettings.AppId);
        Instantiate(testCube);
    }

    public async void OnClickGetMasterServer()
    {
        var res = await HeliosNetwork.GetMvmAddress();
        Debug.Log($"MVM IP : {res}");
    }

    public async void OnClickRoomTaskBtn()
    {
        Debug.Log("[ROOM LIST]");
        // Debug.Log(HeliosNetwork.RoomList);
        foreach (var room in await HeliosNetwork.GetRoomList())
        {
            Debug.Log($"\tRoomID : {room.RoomInfo.RoomID}, RoomName : {room.RoomInfo.Name}");
        }
        GameObject.Find("RoomCreate").GetComponent<Button>().interactable = true;
        GameObject.Find("RoomJoin").GetComponent<Button>().interactable = true;
    }

    public async void OnClickRoomCreate()
    {
        HeliosNetwork.RoomCreateToMaster("TestRoom", joinRoomID);
    }

    public ulong joinRoomID;
    
    public async void OnClickRoomJoin()
    {
        HeliosNetwork.RoomJoinToMaster(joinRoomID);
    }

    public async void OnClickConnectBtn()
    {
        // await HeliosNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Direct to MVS
    /// </summary>
    public void OnClickJoinRoomBtn()
    {
        HeliosNetwork.JoinOrCreateRoom("auth", 1, 1, "Dummy");
    }

    public uint sceneNumber = 1;
    public uint channelID = 1;
    
    public void OnClickJoinGroupBtn()
    {
        HeliosNetwork.JoinGroup(sceneNumber, channelID);
    }

    public void OnClickCreateObjectBrn()
    {
         GameObject cubeObject = HeliosNetwork.Instantiate(prefabsForSpawn[Random.Range(0, prefabsForSpawn.Length)], Vector3.zero, Quaternion.identity);
    }
    public async void OnClickGroupTaskBtn()
    {
        await HeliosNetwork.GetGroupList();
        foreach (var group in HeliosNetwork.GroupList)
        {
            Debug.Log(group.GroupInfo.GroupID.SceneNumber);
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
            case 2:
                HeliosNetwork.HeliosSettings.AppSettings.Protocol = ConnectionProtocol.Udp;
                HeliosNetwork.HeliosSettings.AppSettings.Port = 30083;
                Debug.Log($"Changed To Udp {HeliosNetwork.HeliosSettings.AppSettings.Port}");
                break;
        }
    }

    public override void OnConnected()
    {
        Debug.Log("OnConnected");
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
        Debug.Log($"JoinedRoom : {HeliosNetwork.CurrentRoom.RoomInfo.RoomID}");
        Txt_UserId.text = $"UserId : {HeliosNetwork.LocalPlayer.UserId}";
        GameObject.Find("GroupJoinBtn").GetComponent<Button>().interactable = true;
        GameObject.Find("GroupTaskBtn").GetComponent<Button>().interactable = true;
    }

    public override void OnJoinedRoomFailed(string message)
    {
        
    }

    public override void OnLeftRoom()
    {
        
    }

    public override void OnCreatedGroupFailed(string message)
    {
        
    }

    public override void OnJoinedGroup()
    {
        Debug.Log(HeliosNetwork.IsMasterClient);
        Debug.Log($"Joined Group {HeliosNetwork.CurrentGroup.GroupInfo.GroupID.SceneNumber}");
        Txt_IsMaster.text = $"IsMaster? : {HeliosNetwork.CurrentGroup.IsLocalGroupOwner}";
        GameObject.Find("CreateObject").GetComponent<Button>().interactable = true;
    }

    public override void OnJoinedGroupFailed(string message)
    {
        
    }

    public override void OnLeftGroup()
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName);
        Debug.Log(HeliosNetwork.PlayerList.Count);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName);
        Debug.Log(HeliosNetwork.PlayerList.Count);
    }

    public override void OnPlayerEnteredGroup(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName);
        Debug.Log(HeliosNetwork.PlayerList.Count);
        RPC("AddScore", targetPlayerIDs: null, 3, 6);
    }

    public override void OnPlayerLeftGroup(Player leftPlayer)
    {
        Debug.Log(leftPlayer.NickName);
    }

    public override void OnEvent(EventData eventData)
    {
        
    }

    public override void OnErrorInfo(string errorInfo)
    {
        
    }

    public override void OnMasterClientSwitched(Player masterClient)
    {
        Debug.Log($"NickName : {masterClient.NickName}, {masterClient.PlayerInfo.PlayerID}");
        Debug.Log($"Am I Master? {HeliosNetwork.CurrentGroup.IsLocalGroupOwner}");
    }

    public override void OnObjectInstantiated(ObjectInfo objectInfo)
    {
        Debug.Log(objectInfo.ObjectID);
    }

    public override void OnObjectDestroyed(ObjectInfo objectInfo)
    {
        Debug.Log(objectInfo.ObjectID);
    }

    public override void OnCreatedGroup()
    {
        
    }

    public override void OnConnectedToMasterServer()
    {
        Debug.Log("OnConnectedToMasterServer");
        GameObject.Find("GetRoomInfo").GetComponent<Button>().interactable = true;
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            RPC(nameof(PendulumRPC), null);
            RPC("AddScore", targetPlayerIDs: null, 3, 6);
            // RPC("AddScore", targetPlayerIDs: new uint[] {100, 200, 300}, 3, 6);
            // AddScore();
            AddTest();
            RPC(nameof(ChangeCostume), null, costume);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            RPC("ChangeVec2");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            HeliosNetwork.Disconnect();
        }
    }
}
