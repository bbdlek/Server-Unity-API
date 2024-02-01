using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "MVS/MVS Network Config", fileName = "MVSNetworkConfig")]
[Serializable]
public class MVSNetworkConfig : ScriptableObject
{
    public string mvsVersion;

    public static MVSNetworkConfig Global;

    private void OnEnable()
    {
        if (Global == null)
        {
            Global = this;
        }
    }
}

