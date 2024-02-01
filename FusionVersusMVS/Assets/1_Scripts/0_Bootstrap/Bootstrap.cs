using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEditor;

public class Bootstrap : Singleton<Bootstrap>
{
    public bool enableFusion;
    public bool enableMvs;

    public FusionStarter fusionStarter;
    public MVSStarter mvsStarter;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if(enableFusion) SetUpFusionStarter();
        if(enableMvs) SetUpMvsStarter();
    }

    //Photon Fusion
    #region Fusion

    [Header("Fusion Config")] [SerializeField]
    private GameMode gameMode;

    public GameObject fusionPlayerPrefab;

    private void SetUpFusionStarter()
    {
        GameObject fusionStarter = new GameObject
        {
            name = "FusionStarter"
        };
        fusionStarter.transform.SetParent(transform);
        this.fusionStarter = fusionStarter.AddComponent<FusionStarter>();
    }

    #endregion
    
    
    //MVS
    #region MVS

    private void SetUpMvsStarter()
    {
        GameObject mvsStarter = new GameObject
        {
            name = "MVSStarter",
        };
        mvsStarter.transform.SetParent(transform);
        this.mvsStarter = mvsStarter.AddComponent<MVSStarter>();
    }

    #endregion
}
