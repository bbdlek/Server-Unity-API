using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "MVS/MVS Network Config", fileName = "MVSNetworkConfig")]
[Serializable]
public class MVSNetworkConfig : ScriptableObject
{
    public static MVSNetworkConfig Global;
    
    public string mvsVersion;
    
    public MVSNetworkObjectTable NetworkObjectTable = new MVSNetworkObjectTable();

    private void OnEnable()
    {
        if (Global == null)
        {
            Global = this;
        }
    }
}

