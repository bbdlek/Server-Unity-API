using System;
using MVS.Helios;
using UnityEngine;

public class CubeManager : HeliosMonoBehavior
{
    [HeliosRPC]
    public void AddScore()
    {
        Debug.Log("AddScore");
        score++;
    }

    public int score = 1;
    [HNSync, OnChanged(nameof(OnChangeScore2))] public int score2 = 3;

    private void OnChangeScore2()
    {
        Debug.Log($"OnChangeScore2 {score2}");
    }

    public override void Awake()
    {
        base.Awake();
        Debug.Log(IsMine);
        if(IsMine)
            RPC("AddScore");
    }

    private void OnEnable()
    {
        Debug.Log("Im Enbled");
    }

    private void Start()
    {
        if(IsMine)
            RPC("AddScore");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && IsMine)
        {
            RPC("AddScore");
        }
        
        if (Input.GetKeyDown(KeyCode.L) && IsMine)
        {
            score2++;
        }
    }
    

}
