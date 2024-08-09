using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;
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
        
        // 파라미터들을 byte[]로 직렬화
        public static byte[] SerializeParameters(params object[] parameters)
        {
            List<byte[]> serializedParams = new List<byte[]>();
            foreach (object parameter in parameters)
            {
                byte[] serializedParam = ToTypedJson(parameter);
                serializedParams.Add(serializedParam);
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                foreach (byte[] param in serializedParams)
                {
                    // 각 파라미터의 길이를 먼저 쓰기
                    byte[] lengthBytes = BitConverter.GetBytes(param.Length);
                    memoryStream.Write(lengthBytes, 0, lengthBytes.Length);
                    // 파라미터 데이터 쓰기
                    memoryStream.Write(param, 0, param.Length);
                }
                return memoryStream.ToArray();
            }
        }

        // byte[]를 파라미터들로 역직렬화
        public static object[] DeserializeParameters(byte[] data)
        {
            List<object> parameters = new List<object>();
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                while (memoryStream.Position < memoryStream.Length)
                {
                    // 각 파라미터의 길이를 읽기
                    byte[] lengthBytes = new byte[sizeof(int)];
                    memoryStream.Read(lengthBytes, 0, lengthBytes.Length);
                    int paramLength = BitConverter.ToInt32(lengthBytes, 0);

                    // 파라미터 데이터를 읽기
                    byte[] paramBytes = new byte[paramLength];
                    memoryStream.Read(paramBytes, 0, paramBytes.Length);

                    // 파라미터를 역직렬화
                    object param = FromTypedJson(paramBytes);
                    parameters.Add(param);
                }
            }
            return parameters.ToArray();
        }

        [System.Serializable]
        public class TypedJson
        {
            public string type;
            public string json;
        }
    
        public static byte[] ToTypedJson<T>(T obj)
        {
            string json = String.Empty;
            
            // 타입 정보 가져오기
            string type = obj.GetType().AssemblyQualifiedName;

            if (obj is Color color)
            {
                json = ColorUtility.ToHtmlStringRGBA(color);
            }
            else if (obj is Color32 color32)
            {
                json = ColorUtility.ToHtmlStringRGBA(color32);
            }
            else
            {
                // 객체를 JSON으로 직렬화
                json = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }
        
            // TypedJson 객체 생성 및 직렬화
            TypedJson typedJson = new TypedJson { type = type, json = json };
            string typedJsonString = JsonConvert.SerializeObject(typedJson);

            // string을 byte[]로 변환
            return Encoding.UTF8.GetBytes(typedJsonString);
        }

        public static object FromTypedJson(byte[] typedJsonBytes)
        {
            // byte[]를 string으로 변환
            string typedJsonString = Encoding.UTF8.GetString(typedJsonBytes);
            
            // TypedJson 객체 역직렬화
            TypedJson parsedTypedJson = JsonConvert.DeserializeObject<TypedJson>(typedJsonString);
        
            // 타입 정보 가져오기
            Type parsedType = Type.GetType(parsedTypedJson.type);
        
            if (parsedType == null)
            {
                Debug.LogError("Type information could not be found.");
                return null;
            }

            // JSON을 적절한 타입으로 역직렬화
            try
            {
                if (parsedType == typeof(Color) || parsedType == typeof(Color32))
                {
                    string colorString = parsedTypedJson.json;
                    if (parsedType == typeof(Color))
                    {
                        ColorUtility.TryParseHtmlString("#" + colorString, out Color color);
                        return color;
                    }

                    if (parsedType == typeof(Color32))
                    {
                        ColorUtility.TryParseHtmlString("#" + colorString, out Color color32);
                        return (Color32)color32;
                    }
                }
                else
                {
                    return JsonConvert.DeserializeObject(parsedTypedJson.json, parsedType, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError($"JSON serialization error: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during deserialization: {e.Message}");
                return null;
            }

            return null;
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
            if (!CheckObjectsEqual(list1[i], list2[i]))
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

            if (!CheckObjectsEqual(entry.Value, dict2[entry.Key]))
            {
                return false;
            }
        }

        // 모든 키-값 쌍이 같으면 true 반환
        return true;
    }

    private static bool CheckObjectsEqual(object obj1, object obj2)
    {
        // 객체가 둘 다 리스트인 경우
        if (obj1 is IList && obj2 is IList)
        {
            return CheckListEquals(obj1, obj2);
        }

        // 객체가 둘 다 딕셔너리인 경우
        if (obj1 is IDictionary && obj2 is IDictionary)
        {
            return CheckDictionariesEqual(obj1, obj2);
        }

        // 기본 객체 비교
        return object.Equals(obj1, obj2);
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