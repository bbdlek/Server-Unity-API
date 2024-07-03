using System;
using MVS.Helios;
using UnityEngine;

public class CubeManager : HeliosMonoBehavior
{
    [HeliosRPC("ALL")]
    public void AddScore()
    {
        Debug.Log("AddScore");
        score++;
    }

    public int score = 1;
    [HNSync] public int score2 = 3;

    private void Start()
    {
        // if(IsMine)
            // RPC("AddScore");
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
