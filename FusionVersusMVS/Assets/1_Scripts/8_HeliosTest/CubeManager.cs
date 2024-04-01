using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using UnityEngine;

public class CubeManager : HeliosMonoBehavior
{
    public HNInt score = new HNInt(1);
    public HNInt score2 = new HNInt(3);
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
            Debug.Log(CustomVariables.GetNameByKey(1));
            
            score.Value++;
            score2.Value++;
        }
    }
}
