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
    public Dictionary<int, Color> testStringDic = new Dictionary<int, Color>();

    public Color co = new Color();

    (string, string) ExtractAndLogTypes(string input)
    {
        Debug.Log(input);

        // JSON 문자열 파싱
        var jsonObject = JObject.Parse(input);

        // "$type" 필드에서 타입 정보를 추출
        var typeString = jsonObject["$type"]?.ToString();

        if (string.IsNullOrEmpty(typeString))
        {
            Debug.Log("No type information found.");
            return (null, null);
        }

        // 중첩된 Dictionary를 정확하게 찾기 위한 문자열 분석
        // 가장 바깥쪽 Dictionary의 키와 값 타입을 찾기 위해 구조를 분석
        string keyType = null;
        string valueType = null;

        // `Dictionary`의 타입 문자열에서 'Key'와 'Value' 부분을 찾아내기
        var dictionaryStart = typeString.IndexOf("Dictionary`2");
        if (dictionaryStart != -1)
        {
            var content = typeString.Substring(dictionaryStart);
            var keyStart = content.IndexOf("[[") + 2;
            var keyEnd = content.IndexOf("],", keyStart);
            keyType = content.Substring(keyStart, keyEnd - keyStart).Trim();

            var valueStart = keyEnd + 3;
            // Value 뒤에 있는 마지막 ']]'로 끝나는 위치를 정확히 찾기
            var valueEnd = content.Length - 2;
            valueType = content.Substring(valueStart, valueEnd - valueStart).Trim();
        }

        Debug.Log($"Extracted Key Type: {keyType}");
        Debug.Log($"Extracted Value Type: {valueType}");

        return (keyType, valueType);
    }

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
        Vector2Int vector2Int = new Vector2Int(1, 2);
        Dictionary<int, Vector3> innerDic = new Dictionary<int, Vector3>() { {1, Vector3.back } };
        // var testVal = new Vector2Int()
        // {
        //     x = 10,
        //     y = 1,
        // };
        // Debug.Log(testVal.Serialize(forceReflected: true).json);
        Dictionary<int, Dictionary<int, Vector3>> testDic = new Dictionary<int, Dictionary<int, Vector3>>() {{1, innerDic}};
        Debug.Log(testDic.Serialize().json);
        testDic.Serialize().Deserialize();
        Debug.Log(testDic.Serialize().ToString());
        Debug.Log(testDic.Serialize().objectReferences);
        SerializationData datd = new SerializationData(innerDic.Serialize().json, innerDic.Serialize().objectReferences);
        SerializationData datDic = new SerializationData(testDic.Serialize().json);
        Debug.Log(HeliosUtility.CheckDictionariesEqual(innerDic, datd.Deserialize()));
         
        
        Debug.Log(HeliosUtility.CheckDictionariesEqual(innerDic, BytesToObject(ObjectToBytes(innerDic))));
        return;
        Debug.Log(testDic.Serialize().json);
        var jObjectDic = JObject.Parse(testDic.Serialize().json);
        JArray contentArray = (JArray)jObjectDic["$content"];
        foreach (JObject item in contentArray)
        {
            // Key와 Value 추출
            int key = (int)item["Key"];
            JObject valueObject = (JObject)item["Value"];
            float x = (float)valueObject["x"];
            float y = (float)valueObject["y"];
            float z = (float)valueObject["z"];

            // 결과 출력
            Debug.Log("Key: " + key);
            Debug.Log("Value - x: " + x + ", y: " + y + ", z: " + z);
        }
        string typeDic = jObjectDic["$type"]?.ToString();
        Debug.Log(typeDic);
        
        Type type = Type.GetType(typeDic);
        object ins = Activator.CreateInstance(type);
        Debug.Log(ins.GetType());

        Type type2 =
            Type.GetType(
                "UnityEngine.Vector3, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        object asd = Activator.CreateInstance(type2);
        Debug.Log(asd.GetType());
        
        
        Color color = Color.cyan;
        var data = color.Serialize().json;
        Debug.Log(data);
        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        var jObject = JObject.Parse(data);
        string typeName = jObject["$type"]?.ToString();
        if (typeName == "UnityEngine.Color")
        {
            Color newColor = new Color(
                jObject["r"].Value<float>(),
                jObject["g"].Value<float>(),
                jObject["b"].Value<float>(),
                jObject["a"].Value<float>()
                );
            co = newColor;
        }
        // co = (Color)data.Deserialize();
        
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
