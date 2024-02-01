using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "MVS/MVS Application Settings", fileName = "MVSAppSettings")]
public class MVSAppSettings : ScriptableObject
{
    [Header("MVS Connection")]
    [Tooltip("Connect With MVS Uri")]
    public string mvsURI;

    [Tooltip("Connect With MVS Port")] 
    public string mvsPort;

    public static MVSAppSettings Global;

    private void OnEnable()
    {
        if (Global == null)
        {
            Global = this;
        }
    }
}
