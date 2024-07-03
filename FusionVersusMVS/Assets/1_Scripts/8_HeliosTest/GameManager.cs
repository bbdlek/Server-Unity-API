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
    [HNSync] public int test;

    [HNSync] public Vector2 testVec2 = new Vector2(2, 3);

    [HNSync] public List<int> testList = new List<int>(){1, 2, 3};

    [HNSync] public Costume costume = default;
    
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

    [HeliosRPC(target: "ALL")]
    public void AddScore(int added1, int added2)
    {
        score2 += added1;
        score3 += added2;
    }

    [HeliosRPC("Player")]
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
        HeliosNetwork.ConnectUsingSettings();
    }

    [Command]
    public void SetAppKey(string appKey)
    {
        HeliosNetwork.HeliosSettings.AppSettings.AppId = appKey;
    }

    public async void OnClickGetMasterServer()
    {
        var res = await HeliosNetwork.GetMvmAddress();
        Debug.Log($"MVM IP : {res}");
    }

    public async void OnClickRoomTaskBtn()
    {
        await HeliosNetwork.GetRoomList();
        Debug.Log("[ROOM LIST]");
        foreach (var room in HeliosNetwork.RoomList)
        {
            Debug.Log($"\tRoomID : {room.RoomInfo.RoomID}, RoomName : {room.RoomInfo.Name}");
        }
        GameObject.Find("RoomCreate").GetComponent<Button>().interactable = true;
        GameObject.Find("RoomJoin").GetComponent<Button>().interactable = true;
    }

    public async void OnClickRoomCreate()
    {
        await HeliosNetwork.RoomCreateToMaster();
    }
    
    public async void OnClickRoomJoin()
    {
        await HeliosNetwork.RoomJoinToMaster();
    }

    public async void OnClickConnectBtn()
    {
        await HeliosNetwork.ConnectUsingSettings();
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
        base.OnConnected();
        Debug.Log("OnConnected");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("JoinedRoom");
        Txt_UserId.text = $"UserId : {HeliosNetwork.LocalPlayer.UserId}";
        GameObject.Find("GroupJoinBtn").GetComponent<Button>().interactable = true;
        GameObject.Find("GroupTaskBtn").GetComponent<Button>().interactable = true;
    }

    public override void OnJoinedGroup()
    {
        base.OnJoinedGroup();
        Debug.Log($"Joined Group {HeliosNetwork.CurrentGroup.GroupInfo.GroupID.SceneNumber}");
        Txt_IsMaster.text = $"IsMaster? : {HeliosNetwork.CurrentGroup.IsLocalGroupOwner}";
        GameObject.Find("CreateObject").GetComponent<Button>().interactable = true;
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

    public override void OnPlayerLeftGroup(Player leftPlayer)
    {
        base.OnPlayerLeftGroup(leftPlayer);
        Debug.Log(leftPlayer.NickName);
    }

    public override void OnMasterClientSwitched(Player masterClient)
    {
        base.OnMasterClientSwitched(masterClient);
        Debug.Log($"NickName : {masterClient.NickName}, {masterClient.PlayerInfo.PlayerID}");
        Debug.Log($"Am I Master? {HeliosNetwork.CurrentGroup.IsLocalGroupOwner}");
    }

    public override void OnConnectedToMasterServer()
    {
        base.OnConnectedToMasterServer();
        Debug.Log("OnConnectedToMasterServer");
        GameObject.Find("GetRoomInfo").GetComponent<Button>().interactable = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RPC("AddScore", targetPlayerIDs: null, 3, 6);
            // AddScore();
            AddTest();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            RPC("ChangeVec2", new uint[] { (uint)HeliosNetwork.LocalPlayer.UserId });
        }
    }
}
