using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
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
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(memoryStream, parameters);
                    return memoryStream.ToArray();
                }
            }
            catch (SerializationException e)
            {
                Debug.LogError($"직렬화 가능한 변수만이 파라미터에 들어갈 수 있습니다. Serialization failed: {e.Message}");

                // Log details about each parameter
                for (int i = 0; i < parameters.Length; i++)
                {
                    Debug.LogError($"Parameter {i}: {parameters[i]} (Type: {parameters[i]?.GetType().Name})");
                }

                // Optionally rethrow the exception or handle it as needed
                throw;
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
            using (MemoryStream memoryStream = new MemoryStream(buffer.ToByteArray()))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        public static bool CheckListEquals(object obj1, object obj2)
        {
            // 두 객체가 모두 리스트인지 확인
            if (!(obj1 is IList) || !(obj2 is IList))
            {
                throw new ArgumentException("Both parameters must be lists.");
            }

            IList list1 = (IList)obj1;
            IList list2 = (IList)obj2;

            // 리스트의 길이가 같은지 확인
            if (list1.Count != list2.Count)
            {
                return false;
            }

            // 각 요소를 비교하여 내용이 같은지 확인
            for (int i = 0; i < list1.Count; i++)
            {
                if (!object.Equals(list1[i], list2[i]))
                {
                    return false;
                }
            }

            // 모든 요소가 같으면 true 반환
            return true;
        }
        
        public static bool IsListType(Type type)
        {
            return type.FullName != null && type.FullName.StartsWith("System.Collections.Generic.List`");
        }

        public static bool IsDictionaryType(Type type)
        {
            return type.FullName != null && type.FullName.StartsWith("System.Collections.Generic.Dictionary`");
        }
    }
}