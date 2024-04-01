using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TestGameManager : HeliosMonoBehavior
{
    public HNInt score = new HNInt(0);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            score.Value++;
        }
    }
}
