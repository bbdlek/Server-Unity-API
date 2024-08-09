using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MVS.Helios;
using MVS.Helios.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestHNSync : HeliosMonoBehavior
{
    [HNSync, OnChanged(nameof(OnChangedMastersComment))]
    public string mastersComment = "";
    [HNSync, OnChanged(nameof(OnChangedTestDic))]
    public Dictionary<int, string> testStringDic = new Dictionary<int, string>();
    
    private byte[] ObjectToBytes(object obj)
    {
        var jsonString = obj.Serialize().json;
        return Encoding.UTF8.GetBytes(jsonString);
    }

    private object BytesToObject(byte[] bytes)
    {
        object result = null;
        var jsonString = Encoding.UTF8.GetString(bytes);
        return new SerializationData(jsonString).Deserialize();
    }

    private enum TestEnum
    {
        Hello, World
    }

    private void Start()
    {
        // GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Debug.Log(go.Serialize().json);
        // Debug.Log(go.Serialize().Deserialize());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (HeliosNetwork.IsMasterClient)
            {
                InitTestDic();
            }
        }
        
        return;
        
        if (testStringDic.ContainsKey(10))
        {
            Debug.Log($"[Update] _testStringDic[10]: {testStringDic[10]}");
        }
        else
        {
            Debug.Log("[Update] 아직 Test Dic 키 없음");
        }

        if (!string.IsNullOrEmpty(mastersComment))
        {
            Debug.Log($"[Update] mastersComment: {mastersComment}");
        }
        else
        {
            Debug.Log("[Update] 아직 mastersComment 없음");
        }
    }

    private void InitTestDic()
    {
        mastersComment = "졸림" + Random.Range(0, 1f);;
        testStringDic[10] = mastersComment;
        Debug.Log("InitTestDic 완료");
    }

    private void OnChangedTestDic()
    {
        if(testStringDic.ContainsKey(10))
            Debug.Log($"OnChangedTestDic => testStringDic[10]: {testStringDic[10]}");
    }

    private void OnChangedMastersComment()
    {
        Debug.Log($"OnChangedMastersComment => mastersComment: {mastersComment}");
    }
}
