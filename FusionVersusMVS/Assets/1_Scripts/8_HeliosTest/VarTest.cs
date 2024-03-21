using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MVS.Helios;
using MVS.Realtime;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace _1_Scripts._8_HeliosTest
{
    public class VarTest : MonoBehaviour
    {
        [SerializeField]
        private HNInt score = new HNInt(1);
        [SerializeField]
        private HNInt score2 = new HNInt(2);

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                score.Value++;
            }
        }
    }
}