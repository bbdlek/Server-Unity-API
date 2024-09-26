using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MVS.Helios;
using MVS.Helios.Utility;
using UnityEngine;

public class BakeTestSceneScript : MonoBehaviour
{
    public GameObject bakeCubePrefab;

    private void Awake()
    {
        var go = Instantiate(bakeCubePrefab);
        foreach (var kvp in go.GetComponent<BakeTest>().RPCMethods)
        {
            Debug.Log(kvp.Value.Item1.Name);
        }
    }
}
