using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MVS.Realtime;
using UnityEngine;

namespace _1_Scripts._0_Bootstrap
{
    public class VarTest : MonoBehaviour
    {
        public HNInt score = 2;

        private void Start()
        {
            var bytes = Serialize(score);
            
            Debug.Log(Deserialize<HNInt>(bytes).Value);
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