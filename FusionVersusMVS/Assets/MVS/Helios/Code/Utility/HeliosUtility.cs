using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using UnityEngine;

namespace MVS.Helios.Utility
{
    public static class HeliosUtility
    {
        public static ulong Compute64BitHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // 문자열을 바이트 배열로 변환
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                // 해시 계산
                byte[] hash = sha256.ComputeHash(bytes);
                // 해시의 첫 8바이트를 64비트 정수로 변환
                ulong hash64Bit = BitConverter.ToUInt64(hash, 0);
                return hash64Bit;
            }
        }
        
        public static byte[] SerializeParameters(params object[] parameters)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, parameters);
                return memoryStream.ToArray();
            }
        }

        public static object[] DeserializeParameters(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (object[])binaryFormatter.Deserialize(memoryStream);
            }
        }
        
        public static ByteString ObjectToBytes(object obj)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            if(obj.GetType().IsSerializable)
                binFormatter.Serialize(mStream, obj);
            else
            {
                UnityEngine.Debug.Log("Can't Serialize");
                return null;
            }

            return ByteString.CopyFrom(mStream.ToArray());
            
            // int iSize = Marshal.SizeOf(obj);
            //
            // byte[] arr = new byte[iSize];
            //
            // IntPtr ptr = Marshal.AllocHGlobal(iSize);
            // Marshal.StructureToPtr(obj, ptr, false);
            // Marshal.Copy(ptr, arr, 0, iSize);
            // Marshal.FreeHGlobal(ptr);
            //
            // return ByteString.CopyFrom(arr);
        }

        public static object ByteToObject(ByteString buffer)
        {
            Debug.Log(buffer.Length);
            using (MemoryStream memoryStream = new MemoryStream(buffer.ToByteArray()))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}