using System;
using System.Collections;
using System.Collections.Generic;
using _1_Scripts._8_HeliosTest;
using MVS.Helios;
using MVS.Helios.Utility;
using MVS.Realtime;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

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

    [HNSync, OnChanged(nameof(OnChangedList))]
    public List<int> testInt = new List<int>();

    [HNSync, OnChanged(nameof(OnChangedDic))]
    public Dictionary<int, List<Vector3>> testDic = new Dictionary<int, List<Vector3>>();

    private void OnChangedDic()
    {
        Debug.Log("DICCCCC");
        if(testDic.Count > 0)
            Debug.Log(testDic[0][testDic[0].Count-1]);
    }

    [HNSync]
    public Bounds ccolor;

    [HeliosRPC]
    private void TestRPC(Dictionary<int, List<Vector3>> testVec)
    {
        if(testVec.Count > 0)
            Debug.Log(testVec[0][testVec[0].Count-1]);
    }

    public GameObject playerPrefab;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ccolor = new Bounds
            {
                center = new Vector3(1, 2, 3),
                min = new Vector3(10, 20, 30),
                max = new Vector3(10, 30 , Random.Range(1, 100))
            };
            // ColorUtility.TryParseHtmlString("#" + ColorUtility.ToHtmlStringRGBA(color), out ccolor);
            // Debug.Log(ccolor);
            // ColorUtility.TryParseHtmlString("#" + HeliosUtility.FromTypedJson(data), out var newColor);
            // Debug.Log(newColor);


            
            // RPC(nameof(Test), null, 10, Vector3.down);
            // HeliosNetwork.Disconnect();
            List<Vector3> innderDic = new List<Vector3>();
            innderDic.Add(new Vector3(1, 2, Random.Range(1, 100)));
            testDic[0] = innderDic;
            RPC(nameof(TestRPC), null, testDic);
            // testInt.Add(testInt.Count + 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            HeliosNetwork.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    private void OnChangedList()
    {
        Debug.Log(testInt.Count);
    }

    [HeliosRPC]    
    private void Test(int a, Vector3 b)
    {
        Debug.Log($"{a}, {b}");
        _testDefault.Test();
    }

    public void OnConnectedToMasterServer()
    {
        Debug.Log("Hello??");
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
