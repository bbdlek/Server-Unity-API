using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using UnityEngine;

public class TestHNSync : HeliosMonoBehavior
{
    [HNSync, OnChanged(nameof(OnChangedMastersComment))]
    public string mastersComment = "";
    [HNSync, OnChanged(nameof(OnChangedTestDic))]
    public Dictionary<int, Color> testStringDic = new Dictionary<int, Color>();

    private void Start()
    {
        Debug.Log(HeliosNetwork.IsMasterClient);
        if (HeliosNetwork.IsMasterClient)
        {
            InitTestDic();
        }
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
        testStringDic[10] = new Color(Random.Range(0, 255), 0, 0);
        mastersComment = "졸림" + Random.Range(0, 1f);;
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
