using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using UnityEngine;

public class CubeManager : HeliosMonoBehavior
{
    public HNInt score = new HNInt(1);
    // Start is called before the first frame update
    void Start()
    {
        score.Value = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && IsMine)
        {
            Debug.Log(IsMine);
            score.Value++;
        }
    }
}
