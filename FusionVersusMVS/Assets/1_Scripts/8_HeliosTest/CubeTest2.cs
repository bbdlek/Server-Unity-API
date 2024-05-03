using System;
using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using UnityEngine;

public class CubeTest2 : HeliosMonoBehavior
{
    [HNSync] public float testFloat = 0;
    [HNSync] public float testFloat2 = 0;
    [HNSync] public Vector2 testVec1 = Vector2.zero;
    [HNSync] public Vector2 testVec2 = Vector2.one;
    [HNSync] public bool testBool = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && IsMine)
        {
            testFloat++;
            testFloat2--;
            // testVec1.x++;
            testVec2.y--;
            testBool = !testBool;
        }
    }
}
