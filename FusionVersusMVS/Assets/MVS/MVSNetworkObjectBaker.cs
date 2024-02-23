using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class MVSNetworkObjectBaker
{
    public static void BakeNew()
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            MVSNetworkObject mvsNetworkObject = rootObject.GetComponent<MVSNetworkObject>();
            if (mvsNetworkObject != null)
            {
                Debug.Log(rootObject.name);
                Bake(mvsNetworkObject);
            }
            
        }
    }
    
    public static void Bake(MVSNetworkObject mvsNetworkObject)
    {
        if(!MVSNetworkConfig.Global.NetworkObjectTable._mvsNetworkObjects.Contains(mvsNetworkObject))
        {
            // GUID 주고, 어떤 프리팹인지 주고?

            MVSNetworkConfig.Global.NetworkObjectTable._mvsNetworkObjects.Add(mvsNetworkObject);
        }
    }

    static MVSNetworkObjectBaker()
    {
        EditorSceneManager.sceneSaved += OnSceneSaving;
    }

    private static void OnSceneSaving(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            MVSNetworkObject mvsNetworkObject = rootObject.GetComponent<MVSNetworkObject>();
            if (mvsNetworkObject != null)
            {
                Debug.Log(rootObject.name);
                Bake(mvsNetworkObject);
            }
            
        }
    }
    
}
