using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MVS;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("MVS/Network Object")]
[DisallowMultipleComponent]
public class MVSNetworkObject : MonoBehaviour
{
    [HideInInspector]
    public MVSRunner runner;

    private Guid _networkObjectID;

    public bool HasInputAuthority
    {
        get => runner != null;
    }

    public bool HasStateAuthority
    {
        get => runner != null;
    }

    [Header("Prefab Settings")]
    public bool isSpawnable;

    private void PublishGUID()
    {
        _networkObjectID = Guid.NewGuid();
    }

    private void OnEnable()
    {
        Debug.Log($"{gameObject.name}의 ID : {gameObject.GetInstanceID()}");
        Debug.Log($"{EditorUtility.InstanceIDToObject(gameObject.GetInstanceID())}");
    }
}
