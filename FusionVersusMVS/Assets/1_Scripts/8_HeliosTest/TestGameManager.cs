using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TestGameManager : HeliosMonoBehavior
{
    [HNSync] public int score = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            score++;
        }
    }
}
