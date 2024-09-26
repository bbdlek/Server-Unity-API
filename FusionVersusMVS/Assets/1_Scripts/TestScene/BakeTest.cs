using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVS.Helios;
using MVS.Helios.Utility;
using UnityEngine;

[Serializable]
public struct RPCMethodEntry
{
    public ulong hash;
    public string methodName;
    public BakeTest owner;

    public RPCMethodEntry(ulong hash, string methodName, BakeTest owner)
    {
        this.hash = hash;
        this.methodName = methodName;
        this.owner = owner;
    }
}

[ExecuteInEditMode]
public class BakeTest : MonoBehaviour
{
    // 직렬화 가능한 리스트
    // [HideInInspector]
    [SerializeField]
    private List<RPCMethodEntry> serializedRPCMethods = new List<RPCMethodEntry>();
    
    // 런타임에 사용할 Dictionary (직렬화되지 않음)
    public Dictionary<ulong, Tuple<MethodInfo, BakeTest>> RPCMethods = 
        new Dictionary<ulong, Tuple<MethodInfo, BakeTest>>();

    [HeliosRPC]
    public void TestRPC()
    {
    }

    private void Awake()
    {
        // 직렬화된 데이터로부터 Dictionary 복원
        RestoreRPCMethodsFromSerializedData();
    }

    private void OnValidate()
    {
        RPCMethods.Clear();
        serializedRPCMethods.Clear();
        FindRPCMethods();
    }

    public void FindRPCMethods()
    {
        Type classType = GetType();
        MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            HeliosRPCAttribute attribute = 
                (HeliosRPCAttribute)Attribute.GetCustomAttribute(method, typeof(HeliosRPCAttribute));

            if (attribute != null)
            {
                string methodName = method.Name;
                var hash = HeliosUtility.Compute64BitHash(methodName);

                if (!RPCMethods.ContainsKey(hash))
                {
                    RPCMethods.Add(hash, Tuple.Create(method, this));

                    // 직렬화 리스트에 추가
                    serializedRPCMethods.Add(new RPCMethodEntry(hash, methodName, this));
                }
            }
        }
    }

    private void RestoreRPCMethodsFromSerializedData()
    {
        foreach (var entry in serializedRPCMethods)
        {
            MethodInfo method = GetType().GetMethod(entry.methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                if(!RPCMethods.ContainsKey(entry.hash))
                    RPCMethods.Add(entry.hash, Tuple.Create(method, entry.owner));
            }
        }
    }
}
