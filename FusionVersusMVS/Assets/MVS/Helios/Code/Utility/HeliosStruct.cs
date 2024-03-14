using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocol;
using UnityEngine;
using Type = System.Type;

namespace MVS.Helios.Utility
{
    public class HeliosStruct : MonoBehaviour
    {
        private static int _registerIdx = 1;
        
        private Dictionary<int, Type> _customStructDic = new Dictionary<int, Type>();

        [Serializable]
        struct Player
        {
            public float hp;
            public float mp;
            public string nickName;

            public Player(float hp, float mp, string nickName)
            {
                this.hp = hp;
                this.mp = mp;
                this.nickName = nickName;
            }
        }

        private void Awake()
        {
            RegisterType(typeof(Player));
        }

        private void RegisterType(Type type)
        {
            _customStructDic.Add(_registerIdx, type);
            _registerIdx++;
            Debug.Log($"RegisterType : {type}");
        }

        private void Start()
        {
            
            //TEST 1
            Player player = new Player(10, 10, "nickName");
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            formatter.Serialize(stream, player);
            Debug.Log(stream.Length);
            stream.Close();
            CustomStruct customStruct = SerializeCustomStruct(player);
            var data = DeserializeCustomStruct(customStruct);
            customStruct.ToByteArray();
            Player player2 = (Player)data;
            Debug.Log($"HP : {player2.hp}, MP : {player2.mp}, NICKNAME : {player2.nickName}");
            
            //TEST2
            CustomStruct testStruct = new CustomStruct();
            // CustomStruct testStruct = customStruct;
            testStruct.Data.Add(Any.Pack(new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = 0,
                    InstanceID = 1234
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = 0
            }));
            var pkt = new C_EVENT
            {
                EventCode = 0
            };
            pkt.CustomData.Add(testStruct);
            // testStruct.Data.Insert(3, Any.Pack(new FloatValue { Value = 32 }));
            for (int i = 0; i < testStruct.Data.Count; i++)
            {
                var anyData = testStruct.Data[i];
                if (testStruct.Data[i] == null)
                {
                    Debug.Log($"Index : {i}, Value : No Value");
                }
                if (anyData.Is(Int32Value.Descriptor))
                {
                    Int32Value intValue = anyData.Unpack<Int32Value>();
                    Debug.Log($"Index : {i}, Value : {intValue.Value}");
                }
                if (anyData.Is(FloatValue.Descriptor))
                {
                    FloatValue floatValue = anyData.Unpack<FloatValue>();
                    Debug.Log($"Index : {i}, Value : {floatValue.Value}");
                }
                if (anyData.Is(StringValue.Descriptor))
                {
                    StringValue stringValue = anyData.Unpack<StringValue>();
                    Debug.Log($"Index : {i}, Value : {stringValue.Value}");
                }
                if (anyData.Is(BoolValue.Descriptor))
                {
                    BoolValue boolValue = anyData.Unpack<BoolValue>();
                    Debug.Log($"Index : {i}, Value : {boolValue.Value}");
                }
                if(anyData.Is(ObjectInfo.Descriptor))
                {
                    ObjectInfo objectInfo = anyData.Unpack<ObjectInfo>();
                    Debug.Log($"Index : {i}, Value : {objectInfo.ObjectID.InstanceID}");
                }
            }
        }

        private CustomStruct SerializeCustomStruct(object obj)
        {
            Type objType = obj.GetType();
            int matchKey = _customStructDic.FirstOrDefault(type => type.Value == objType).Key;
            if (matchKey == 0)
            {
                Debug.Log("Please Register CustomType");
                return null;
            }
            var customStruct = new CustomStruct();
            customStruct.TypeIndex = matchKey;

            FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                customStruct = AddValueToCustomStruct(customStruct, field.GetValue(obj));
            }

            return customStruct;
        }

        private object DeserializeCustomStruct(CustomStruct customStruct)
        {
            int matchKey = customStruct.TypeIndex;
            Type objType = _customStructDic[matchKey];
                
            object obj = Activator.CreateInstance(objType);
            FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            for (int i = 0; i < customStruct.Data.Count; i++)
            {
                var anyData = customStruct.Data[i];
                if (anyData.Is(Int32Value.Descriptor))
                {
                    Int32Value intValue = anyData.Unpack<Int32Value>();
                    fields[i].SetValue(obj, intValue.Value);
                }
                if (anyData.Is(FloatValue.Descriptor))
                {
                    FloatValue floatValue = anyData.Unpack<FloatValue>();
                    fields[i].SetValue(obj, floatValue.Value);
                }
                if (anyData.Is(StringValue.Descriptor))
                {
                    StringValue stringValue = anyData.Unpack<StringValue>();
                    fields[i].SetValue(obj, stringValue.Value);
                }
                if (anyData.Is(BoolValue.Descriptor))
                {
                    BoolValue boolValue = anyData.Unpack<BoolValue>();
                    fields[i].SetValue(obj, boolValue.Value);
                }
            }

            return obj;
        }

        private CustomStruct AddValueToCustomStruct(CustomStruct customStruct, object value)
        {
            switch (value)
            {
                case int intValue:
                    customStruct.Data.Add(Any.Pack(new Int32Value{ Value = intValue}));
                    break;
                case float floatValue:
                    customStruct.Data.Add(Any.Pack(new FloatValue{ Value = floatValue}));
                    break;
                case string stringValue:
                    customStruct.Data.Add(Any.Pack(new StringValue{ Value = stringValue}));
                    break;
                case bool boolValue:
                    customStruct.Data.Add(Any.Pack(new BoolValue{ Value = boolValue}));
                    break;
            }
            
            return customStruct;
        }
    }
}