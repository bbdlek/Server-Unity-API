using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using Unity.VisualScripting;
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
                return Encoding.UTF8.GetBytes(parameters.Serialize().json);
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
            return (object[])new SerializationData(Encoding.UTF8.GetString(data)).Deserialize();
        }

        public static string ObjectToString(object obj)
        {
            return obj.Serialize().json;
        }

        public static object StringToObject(string data)
        {
            return new SerializationData(data).Deserialize();
        }
        
        public static ByteString ObjectToBytes2(object obj)
        {
            var jsonString = obj.Serialize().json;
            Debug.Log(ByteString.CopyFrom(Encoding.UTF8.GetBytes(jsonString)));
            return ByteString.CopyFrom(Encoding.UTF8.GetBytes(jsonString));
        }

        public static object BytesToObject2(ByteString bytes)
        {
            object result = null;
            var jsonString = Encoding.UTF8.GetString(bytes.ToByteArray());
            Debug.Log(bytes.ToByteArray());
            return new SerializationData(jsonString).Deserialize();
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
        
        public static bool CheckDictionariesEqual(object obj1, object obj2)
        {
            // 두 객체가 모두 Dictionary인지 확인
            if (!(obj1 is IDictionary) || !(obj2 is IDictionary))
            {
                throw new ArgumentException("Both parameters must be dictionaries.");
            }

            IDictionary dict1 = (IDictionary)obj1;
            IDictionary dict2 = (IDictionary)obj2;

            // Dictionary의 키-값 쌍의 개수가 같은지 확인
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            // 각 키에 대한 값이 같은지 확인
            foreach (DictionaryEntry entry in dict1)
            {
                if (!dict2.Contains(entry.Key))
                {
                    return false;
                }

                if (!object.Equals(entry.Value, dict2[entry.Key]))
                {
                    return false;
                }
            }

            // 모든 키-값 쌍이 같으면 true 반환
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