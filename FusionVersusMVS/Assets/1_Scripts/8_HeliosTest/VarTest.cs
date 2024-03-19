using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MVS.Realtime;
using Unity.Netcode;
using UnityEngine;

namespace _1_Scripts._8_HeliosTest
{
    public class VarTest : MonoBehaviour
    {
        private HNInt score = new HNInt(1);
        private HNInt score2 = new HNInt(2);
        
        private List<object> _heliosVariables = new List<object>();

        private void Start()
        {

        }

        private void OnValueChanged(int previousvalue, int newvalue)
        {
            Debug.Log($"{previousvalue} -> {newvalue}");
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                score.Value = 3;
                score.Value++;
                score.Value = score2.Value;
            }
        }
        
        // 객체를 바이트 배열로 직렬화하는 메서드
        static byte[] Serialize(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        // 바이트 배열을 객체로 역직렬화하는 메서드
        static T Deserialize<T>(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}