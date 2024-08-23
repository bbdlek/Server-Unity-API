using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using MVS.Helios.Utility;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    [Serializable]
    public struct RPCMethodEntry
    {
        public ulong hash;
        public string methodName;
        public HeliosMonoBehavior owner;

        public RPCMethodEntry(ulong hash, string methodName, HeliosMonoBehavior owner)
        {
            this.hash = hash;
            this.methodName = methodName;
            this.owner = owner;
        }
    }
    
    [Serializable]
    public struct HeliosAttributeEntry
    {
        public int key;
        public string fieldName; // FieldInfo를 문자열로 변환하여 저장
        public string fieldType;

        public HeliosAttributeEntry(int key, string fieldName, string fieldType)
        {
            this.key = key;
            this.fieldName = fieldName;
            this.fieldType = fieldType;
        }
    }
    
    [Serializable]
    public struct HeliosAttributeCallbackEntry
    {
        public int key;
        public string methodName;
        public HeliosMonoBehavior owner;

        public HeliosAttributeCallbackEntry(int key, string methodName, HeliosMonoBehavior owner)
        {
            this.key = key;
            this.methodName = methodName;
            this.owner = owner;
        }
    }
    
    [Serializable]
    public struct AttributeMonoBehaviorEntry
    {
        public int key;
        public HeliosMonoBehavior monoBehavior;

        public AttributeMonoBehaviorEntry(int key, HeliosMonoBehavior monoBehavior)
        {
            this.key = key;
            this.monoBehavior = monoBehavior;
        }
    }
    
    [Serializable]
    public struct InitialHeliosValueEntry
    {
        public int key;
        public byte[] value; // object를 string으로 변환하여 저장

        public InitialHeliosValueEntry(int key, object value)
        {
            this.key = key;
            this.value = HeliosUtility.ToTypedJson(value);
        }
    }
    
    [ExecuteInEditMode]
    public class HeliosMonoBehavior : MonoBehaviour
    {
        public bool hasUpdate
        {
            get
            {
                if (heliosAttributes.Count == 0) return false;
        
                for (int i = CustomVariablesUnity.ScaleKey + 1; i < heliosAttributes.Count + CustomVariablesUnity.ScaleKey + 1; i++)
                {
                    var currentValue = heliosAttributes[i].GetValue(attributeMonoBehaviors[i]);
                    var initialValue = initialHeliosValues[i];

                    if (HeliosUtility.IsListType(heliosAttributes[i].FieldType))
                    {
                        if (!HeliosUtility.CheckListEquals(currentValue, initialValue))
                        {
                            return true;
                        }
                    }
                    else if (HeliosUtility.IsDictionaryType(heliosAttributes[i].FieldType))
                    {
                        if (!HeliosUtility.CheckDictionariesEqual(currentValue, initialValue))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!currentValue.Equals(initialValue))
                            {
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                }
        
                return false;
            }
        }


        public Dictionary<int, FieldInfo> heliosAttributes = new Dictionary<int, FieldInfo>();

        public Dictionary<int, Tuple<string, HeliosMonoBehavior>> heliosAttributeCallbacks =
            new Dictionary<int, Tuple<string, HeliosMonoBehavior>>();
        public Dictionary<int, HeliosMonoBehavior> attributeMonoBehaviors = new Dictionary<int, HeliosMonoBehavior>();
        public Dictionary<int, object> initialHeliosValues = new Dictionary<int, object>();

        public Dictionary<ulong, Tuple<MethodInfo, HeliosMonoBehavior>> RPCMethods =
            new Dictionary<ulong, Tuple<MethodInfo, HeliosMonoBehavior>>();
        
        [HideInInspector]
        [SerializeField]
        private List<HeliosAttributeEntry> serializedHeliosAttributes = new List<HeliosAttributeEntry>();
        
        [HideInInspector]
        [SerializeField]
        private List<HeliosAttributeCallbackEntry> serializedHeliosAttributeCallbacks = new List<HeliosAttributeCallbackEntry>();
        
        [HideInInspector]
        [SerializeField]
        private List<AttributeMonoBehaviorEntry> serializedAttributeMonoBehaviors = new List<AttributeMonoBehaviorEntry>();
        
        [HideInInspector]
        [SerializeField]
        private List<InitialHeliosValueEntry> serializedInitialHeliosValues = new List<InitialHeliosValueEntry>();
        
        [HideInInspector]
        [SerializeField]
        private List<RPCMethodEntry> serializedRPCMethods = new List<RPCMethodEntry>();
        

        public ObjectInfo ObjectInfo = new ObjectInfo
        {
            ObjectID = new ObjectID(),
            SyncType = ObjectSyncType.GroupOwn,
            OwnerPlayerID = 0,
        };
        
        public bool IsMine
        {
            get
            {
                if (this.GetComponentInSelfOrParent<HeliosObject>())
                {
                    return this.GetComponentInSelfOrParent<HeliosObject>().IsMine;
                }
                return false;
            }
        }
        
        private void OnValidate()
        {
            serializedHeliosAttributes.Clear();
            serializedHeliosAttributeCallbacks.Clear();
            serializedAttributeMonoBehaviors.Clear();
            serializedInitialHeliosValues.Clear();
            serializedRPCMethods.Clear();
            FindRPCMethods();
            FindNetworkedVariables();
        }

        public virtual void Awake()
        {
            if(!this.GetComponentInSelfOrParent<HeliosObject>())
            {
                if(!HeliosNetwork.IsInHeliosObjectList(GetType().Name))
                {
                    HeliosNetwork.HeliosObjectList.Add(this);
                    ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(this);
                }
            }
            
            RestoreHeliosAttributeFromSerializedData();
            RestoreHeliosAttributeCallbacksFromSerializedData();
            RestoreAttributeMonoBehaviorsFromSerializedData();
            RestoreInitialHeliosValuesFromSerializedData();
            RestoreRPCMethodsFromSerializedData();
            
            if(ObjectInfo == null)
                ObjectInfo = new ObjectInfo();
            
            // FindNetworkedVariables();
            // FindRPCMethods();
        }

        public void RPC(string methodName, ulong[] targetPlayerIDs = null, params object[] args)
        {
            ObjectID objectID;
            if (this.GetComponentInSelfOrParent<HeliosObject>())
            {
                // Debug.Log($"{methodName} RPC {this.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.ObjectID.ClientInstanceID}");
                objectID = this.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.ObjectID;
            }
            else
            {
                // Debug.Log($"{methodName} RPC {ObjectInfo.ObjectID.ClientInstanceID}");
                // Debug.Log(HeliosNetwork.IsInHeliosObjectList(GetType().Name));
                if(!HeliosNetwork.IsInHeliosObjectList(GetType().Name))
                {
                    HeliosNetwork.HeliosObjectList.Add(this);
                    ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(this);
                }
                objectID = ObjectInfo.ObjectID;
            }

            if (targetPlayerIDs == null)
            {
                targetPlayerIDs = new ulong[] {0};
            }
            
            HeliosNetwork.RPC(objectID, methodName, targetPlayerIDs, args);
        }

        public void ExecuteRpc(ulong methodNameHash, byte[] methodArgs)
        {
            var parameters = HeliosUtility.DeserializeParameters(methodArgs);
            // Debug.Log($"ExecuteRPC : {GetType().Name}");
            if (!RPCMethods.ContainsKey(methodNameHash))
            {
                Debug.LogError($"Does not contain key in {GetType().Name}");
                return;
            }
            var method = RPCMethods[methodNameHash].Item1;
            // Debug.Log($"ExecuteRPC : {method.Name}");
            var ho = RPCMethods[methodNameHash].Item2;
            
            var parameterInfos = method.GetParameters();
            object[] finalParameters = new object[parameterInfos.Length];
            
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                if (i < parameters.Length)
                {
                    finalParameters[i] = parameters[i];
                }
                else
                {
                    // 기본값이 있는 매개변수일 경우 기본값 적용
                    if (parameterInfos[i].IsOptional)
                    {
                        finalParameters[i] = parameterInfos[i].DefaultValue;
                    }
                    else
                    {
                        Debug.LogError($"매개변수 {i}번째에 값이 제공되지 않았으며 기본값도 없습니다.");
                        return;
                    }
                }
            }
            
            try
            {
                method.Invoke(ho, finalParameters);
            }
            catch (Exception e)
            {
                Debug.LogError($"예상된 매개변수 수와 일치하지 않습니다. {e.Message}");
                throw;
            }
        }
        
        public void FindNetworkedVariables()
        {
            // if(_isFindNetworkedVariables) return;
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                HNSyncAttribute attribute =
                    (HNSyncAttribute)Attribute.GetCustomAttribute(field, typeof(HNSyncAttribute));
                if (attribute != null)
                {
                    Protocol.HeliosVariable hv = new Protocol.HeliosVariable();
                    var obj = field.GetValue(this);
                    if (field.FieldType == typeof(int))
                    {
                        hv.NInt32 = (int)obj;
                    }
                    else if(field.FieldType == typeof(long))
                    {
                        hv.NInt64 = (long)obj;
                    }
                    else if(field.FieldType == typeof(float))
                    {
                        hv.NFloat = (float)obj;
                    }
                    else if(field.FieldType == typeof(double))
                    {
                        hv.NDouble = (double)obj;
                    }
                    else if(field.FieldType == typeof(bool))
                    {
                        hv.NBool = (bool)obj;
                    }
                    else if(field.FieldType == typeof(string))
                    {
                        hv.NString = (string)obj;
                    }
                    else if(field.FieldType == typeof(Vector2))
                    {
                        var v = (Vector2)obj;
                        hv.NVector = new Protocol.Vector3
                        {
                            X = v.x,
                            Y = v.y,
                            Z = 0
                        };
                    }
                    else if(field.FieldType == typeof(Vector3))
                    {
                        Vector3 v = (Vector3)obj;
                        hv.NVector = new Protocol.Vector3
                        {
                            X = v.x,
                            Y = v.y,
                            Z = v.z
                        };
                    }
                    else if(field.FieldType == typeof(Quaternion))
                    {
                        Quaternion q = (Quaternion)obj;
                        Vector3 v = q.eulerAngles;
                        hv.NVector = new Protocol.Vector3
                        {
                            X = v.x,
                            Y = v.y,
                            Z = v.z
                        };
                    }
                    else
                    {
                        hv.NCustom = ByteString.CopyFrom(HeliosUtility.ToTypedJson(obj));
                    }
                    if (this.GetComponentInSelfOrParent<HeliosObject>())
                    {
                        var ho = this.GetComponentInSelfOrParent<HeliosObject>();
                        attribute.Owner = ho;
                        
                        //HELIOSVARIABLE
                        var key = ho.ObjectInfo.Values.Count;
                        if (key == 0)
                        {
                            ho.ObjectInfo.Values.Add(new Protocol.HeliosVariable
                            {
                                Key = CustomVariablesUnity.PosKey,
                                NVector = new Protocol.Vector3
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                }
                            });
                            ho.ObjectInfo.Values.Add(new Protocol.HeliosVariable
                            {
                                Key = CustomVariablesUnity.RotKey,
                                NVector = new Protocol.Vector3
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                }
                            });
                            ho.ObjectInfo.Values.Add(new Protocol.HeliosVariable
                            {
                                Key = CustomVariablesUnity.ScaleKey,
                                NVector = new Protocol.Vector3
                                {
                                    X = 1,
                                    Y = 1,
                                    Z = 1
                                }
                            });
                        }
                        key = ho.ObjectInfo.Values.Count;
                        hv.Key = key;
                        if(!ho.ObjectInfo.Values.Contains(hv))
                        {
                            ho.ObjectInfo.Values.Add(hv);
                            // Debug.Log($"ObjectInfo Values {key}, {hv.ValueCase} {field.Name}");
                        }
                        //ATTRIBUTE
                        var attributeItem = new HeliosAttributeEntry(key, field.Name, field.FieldType.ToString());
                        if(!serializedHeliosAttributes.Contains(attributeItem))
                        {
                            // Debug.Log($"{this.GetType().Name}, Add HNSync {field.Name} HO");
                            // ho.heliosAttributes.Add(key, field);
                            serializedHeliosAttributes.Add(attributeItem);
                        }
                        
                        OnChangedAttribute callbackAttribute =
                            (OnChangedAttribute)Attribute.GetCustomAttribute(field, typeof(OnChangedAttribute));
                        if (callbackAttribute != null)
                        {
                            var callbackItem = new HeliosAttributeCallbackEntry(key, callbackAttribute.MethodName, this);
                            if(!serializedHeliosAttributeCallbacks.Contains(callbackItem))
                            {
                                // ho.heliosAttributeCallbacks.Add(key, Tuple.Create(callbackAttribute.MethodName, this));
                                serializedHeliosAttributeCallbacks.Add(callbackItem);
                            }
                        }

                        var attributeMono = new AttributeMonoBehaviorEntry(key, this);
                        if(!serializedAttributeMonoBehaviors.Contains(attributeMono))
                        {
                            // ho.attributeMonoBehaviors.Add(key, this);
                            serializedAttributeMonoBehaviors.Add(attributeMono);
                        }

                        var initialHv = new InitialHeliosValueEntry(key, DeepCopyHelper.DeepCopy(obj));
                        if(!serializedInitialHeliosValues.Contains(initialHv))
                        {
                            // ho.initialHeliosValues.Add(key, DeepCopyHelper.DeepCopy(obj));
                            serializedInitialHeliosValues.Add(initialHv);
                        }
                    }
                    else
                    {
                        attribute.Owner = this;
                        
                        //HELIOSVARIABLE
                        int requiredCount = CustomVariablesUnity.ScaleKey + 2;
                        int currentCount = ObjectInfo.Values.Count;

                        if (currentCount < requiredCount)
                        {
                            int elementsToAdd = requiredCount - currentCount - 1;
                            ObjectInfo.Values.AddRange(Enumerable.Repeat(new HeliosVariable(), elementsToAdd));
                        }
                        
                        var key = ObjectInfo.Values.Count;
                        hv.Key = key;
                        
                        ObjectInfo.Values.Add(hv);
                        
                        //ATTRIBUTE
                        var attributeItem = new HeliosAttributeEntry(key, field.Name, field.FieldType.ToString());
                        if(!serializedHeliosAttributes.Contains(attributeItem))
                        {
                            // Debug.Log($"{this.GetType().Name}, Add HNSync {field.Name} HO");
                            // ho.heliosAttributes.Add(key, field);
                            serializedHeliosAttributes.Add(attributeItem);
                        }
                        
                        OnChangedAttribute callbackAttribute =
                            (OnChangedAttribute)Attribute.GetCustomAttribute(field, typeof(OnChangedAttribute));
                        if (callbackAttribute != null)
                        {
                            var callbackItem = new HeliosAttributeCallbackEntry(key, callbackAttribute.MethodName, this);
                            if(!serializedHeliosAttributeCallbacks.Contains(callbackItem))
                            {
                                // ho.heliosAttributeCallbacks.Add(key, Tuple.Create(callbackAttribute.MethodName, this));
                                serializedHeliosAttributeCallbacks.Add(callbackItem);
                            }
                        }

                        var attributeMono = new AttributeMonoBehaviorEntry(key, this);
                        if(!serializedAttributeMonoBehaviors.Contains(attributeMono))
                        {
                            // ho.attributeMonoBehaviors.Add(key, this);
                            serializedAttributeMonoBehaviors.Add(attributeMono);
                        }

                        var initialHv = new InitialHeliosValueEntry(key, DeepCopyHelper.DeepCopy(obj));
                        if(!serializedInitialHeliosValues.Contains(initialHv))
                        {
                            // ho.initialHeliosValues.Add(key, DeepCopyHelper.DeepCopy(obj));
                            serializedInitialHeliosValues.Add(initialHv);
                        }    
                        
                    }
                }
            }
        }
        

        public void FindRPCMethods()
        {
            // if(_isFindRPC) return;
            Type classType = GetType(); 
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                HeliosRPCAttribute attribute =
                    (HeliosRPCAttribute)Attribute.GetCustomAttribute(method, typeof(HeliosRPCAttribute));
                
                if (attribute != null)
                {
                    string methodName = method.Name;
                    var hash = HeliosUtility.Compute64BitHash(methodName);

                    var item = new RPCMethodEntry(hash, methodName, this);
                    
                    if(!serializedRPCMethods.Contains(item))
                    {
                        // Debug.Log($"{this.GetType().Name}, Add RPC {methodName}");
                        // RPCMethods.Add(hash, Tuple.Create(method, this));

                        // 직렬화 리스트에 추가
                        serializedRPCMethods.Add(item);
                    }
                }
            }
        }
        
        private void RestoreHeliosAttributeFromSerializedData()
        {
            foreach (var entry in serializedHeliosAttributes)
            {
                var field = GetType().GetField(entry.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    Protocol.HeliosVariable hv = new Protocol.HeliosVariable();
                    var obj = field.GetValue(this);
                    if (field.FieldType == typeof(int))
                    {
                        hv.NInt32 = (int)obj;
                    }
                    else if (field.FieldType == typeof(long))
                    {
                        hv.NInt64 = (long)obj;
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        hv.NFloat = (float)obj;
                    }
                    else if (field.FieldType == typeof(double))
                    {
                        hv.NDouble = (double)obj;
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        hv.NBool = (bool)obj;
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        hv.NString = (string)obj;
                    }
                    else if (field.FieldType == typeof(Vector2))
                    {
                        var v = (Vector2)obj;
                        hv.NVector = new Protocol.Vector3
                        {
                            X = v.x,
                            Y = v.y,
                            Z = 0
                        };
                    }
                    else if (field.FieldType == typeof(Vector3))
                    {
                        Vector3 v = (Vector3)obj;
                        hv.NVector = new Protocol.Vector3
                        {
                            X = v.x,
                            Y = v.y,
                            Z = v.z
                        };
                    }
                    else if (field.FieldType == typeof(Quaternion))
                    {
                        Quaternion q = (Quaternion)obj;
                        Vector3 v = q.eulerAngles;
                        hv.NVector = new Protocol.Vector3
                        {
                            X = v.x,
                            Y = v.y,
                            Z = v.z
                        };
                    }
                    else
                    {
                        hv.NCustom = ByteString.CopyFrom(HeliosUtility.ToTypedJson(obj));
                    }


                    if (this.GetComponentInSelfOrParent<HeliosObject>() != null)
                    {
                        var ho = this.GetComponentInSelfOrParent<HeliosObject>();
                        
                        //HELIOSVARIABLE
                        var key = ho.ObjectInfo.Values.Count;
                        if (key == 0)
                        {
                            ho.ObjectInfo.Values.Add(new Protocol.HeliosVariable
                            {
                                Key = CustomVariablesUnity.PosKey,
                                NVector = new Protocol.Vector3
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                }
                            });
                            ho.ObjectInfo.Values.Add(new Protocol.HeliosVariable
                            {
                                Key = CustomVariablesUnity.RotKey,
                                NVector = new Protocol.Vector3
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                }
                            });
                            ho.ObjectInfo.Values.Add(new Protocol.HeliosVariable
                            {
                                Key = CustomVariablesUnity.ScaleKey,
                                NVector = new Protocol.Vector3
                                {
                                    X = 1,
                                    Y = 1,
                                    Z = 1
                                }
                            });
                        }
                        
                        key = ho.ObjectInfo.Values.Count;
                        hv.Key = key;
                        if(!ho.ObjectInfo.Values.Contains(hv))
                        {
                            ho.ObjectInfo.Values.Add(hv);
                            // Debug.Log($"ObjectInfo Values {key}, {hv.ValueCase} {field.Name}");
                        }
                        
                        if (!this.GetComponentInSelfOrParent<HeliosObject>().heliosAttributes.ContainsKey(entry.key))
                        {
                            this.GetComponentInSelfOrParent<HeliosObject>().heliosAttributes.Add(entry.key, field);
                            // Debug.Log(
                            //     $"Restore HNSync {field.Name}. Total Count {this.GetComponentInSelfOrParent<HeliosObject>().heliosAttributes.Count}");
                        }
                    }
                    else
                    {
                        int requiredCount = CustomVariablesUnity.ScaleKey + 2;
                        int currentCount = ObjectInfo.Values.Count;

                        if (currentCount < requiredCount)
                        {
                            int elementsToAdd = requiredCount - currentCount - 1;
                            ObjectInfo.Values.AddRange(Enumerable.Repeat(new HeliosVariable(), elementsToAdd));
                        }
                        
                        var key = ObjectInfo.Values.Count;
                        hv.Key = key;
                        
                        ObjectInfo.Values.Add(hv);
                        
                        if (!heliosAttributes.ContainsKey(entry.key))
                        {
                            heliosAttributes.Add(entry.key, field);
                            // Debug.Log($"Restore HNSync {field.Name}. Total Count {heliosAttributes.Count}");
                        }
                    }

                }
            }
        }
        
        private void RestoreHeliosAttributeCallbacksFromSerializedData()
        {
            if (this.GetComponentInSelfOrParent<HeliosObject>() != null)
            {
                // this.GetComponentInSelfOrParent<HeliosObject>().heliosAttributeCallbacks.Clear();
                foreach (var entry in serializedHeliosAttributeCallbacks)
                {
                    if(!this.GetComponentInSelfOrParent<HeliosObject>().heliosAttributeCallbacks.ContainsKey(entry.key))
                        this.GetComponentInSelfOrParent<HeliosObject>().heliosAttributeCallbacks.Add(entry.key, Tuple.Create(entry.methodName, entry.owner));
                }
            }
            else
            {
                // heliosAttributeCallbacks.Clear(); // 초기화
                foreach (var entry in serializedHeliosAttributeCallbacks)
                {
                    if(!heliosAttributeCallbacks.ContainsKey(entry.key))
                        heliosAttributeCallbacks.Add(entry.key, Tuple.Create(entry.methodName, entry.owner));
                }
            }
        }
        
        private void RestoreAttributeMonoBehaviorsFromSerializedData()
        {
            if (this.GetComponentInSelfOrParent<HeliosObject>() != null)
            {
                // this.GetComponentInSelfOrParent<HeliosObject>().attributeMonoBehaviors.Clear(); // 초기화

                foreach (var entry in serializedAttributeMonoBehaviors)
                {
                    if(!this.GetComponentInSelfOrParent<HeliosObject>().attributeMonoBehaviors.ContainsKey(entry.key))
                        this.GetComponentInSelfOrParent<HeliosObject>().attributeMonoBehaviors.Add(entry.key, entry.monoBehavior);
                }
            }
            else
            {
                // attributeMonoBehaviors.Clear(); // 초기화

                foreach (var entry in serializedAttributeMonoBehaviors)
                {
                    if(!attributeMonoBehaviors.ContainsKey(entry.key))
                        attributeMonoBehaviors.Add(entry.key, entry.monoBehavior);
                }
            }
        }

        private void RestoreInitialHeliosValuesFromSerializedData()
        {
            if (this.GetComponentInSelfOrParent<HeliosObject>() != null)
            {
                // this.GetComponentInSelfOrParent<HeliosObject>().initialHeliosValues.Clear(); // 초기화

                foreach (var entry in serializedInitialHeliosValues)
                {
                    if(!this.GetComponentInSelfOrParent<HeliosObject>().initialHeliosValues.ContainsKey(entry.key))
                        this.GetComponentInSelfOrParent<HeliosObject>().initialHeliosValues.Add(entry.key, HeliosUtility.FromTypedJson(entry.value));
                }
            }
            else
            {
                // initialHeliosValues.Clear(); // 초기화

                foreach (var entry in serializedInitialHeliosValues)
                {
                    if(!initialHeliosValues.ContainsKey(entry.key))
                        initialHeliosValues.Add(entry.key, HeliosUtility.FromTypedJson(entry.value));
                }
            }
        }
        
        private void RestoreRPCMethodsFromSerializedData()
        {
            if (this.GetComponentInSelfOrParent<HeliosObject>() != null)
            {
                foreach (var entry in serializedRPCMethods)
                {
                    MethodInfo method = GetType().GetMethod(entry.methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        if(!this.GetComponentInSelfOrParent<HeliosObject>().RPCMethods.ContainsKey(entry.hash))
                        {
                            this.GetComponentInSelfOrParent<HeliosObject>().RPCMethods
                                .Add(entry.hash, Tuple.Create(method, entry.owner));
                            // Debug.Log($"HO Restore RPC {method.Name}. Total Count {this.GetComponentInSelfOrParent<HeliosObject>().RPCMethods.Count}");
                        }
                    }
                }
            }
            else
            {
                foreach (var entry in serializedRPCMethods)
                {
                    MethodInfo method = GetType().GetMethod(entry.methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                    
                        if(!RPCMethods.ContainsKey(entry.hash))
                        {
                            RPCMethods.Add(entry.hash, Tuple.Create(method, entry.owner));
                            // Debug.Log($"Restore RPC {method.Name}. Total Count {RPCMethods.Count}");
                        }
                    }
                }
            }
        }

        public void UpdateCustomData(ObjectInfo updatedObjectInfo)
        {
            foreach (var customData in updatedObjectInfo.Values)
            {
                var key = customData.Key;
                if(key < 3) continue;
                if(!heliosAttributes.ContainsKey(key)) continue;
                var field = heliosAttributes[key];
                switch (customData.ValueCase)
                {
                    case Protocol.HeliosVariable.ValueOneofCase.NInt32:
                        field.SetValue(attributeMonoBehaviors[key], customData.NInt32);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NInt64:
                        field.SetValue(attributeMonoBehaviors[key], customData.NInt64); 
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NFloat:
                        field.SetValue(attributeMonoBehaviors[key], customData.NFloat);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NBool:
                        field.SetValue(attributeMonoBehaviors[key], customData.NBool);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NDouble:
                        field.SetValue(attributeMonoBehaviors[key], customData.NDouble);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NString:
                        field.SetValue(attributeMonoBehaviors[key], customData.NString);
                        break;
                    case Protocol.HeliosVariable.ValueOneofCase.NVector:
                        if (field.FieldType == typeof(Vector2))
                        {
                            Vector2 val =
                                new Vector2(
                                    (float)customData.NVector.X,
                                    (float)customData.NVector.Y);
                            field.SetValue(attributeMonoBehaviors[key], val);
                        } else if (field.FieldType == typeof(UnityEngine.Vector3))
                        {
                            UnityEngine.Vector3 val =
                                new Vector3(
                                    (float)customData.NVector.X,
                                    (float)customData.NVector.Y,
                                    (float)customData.NVector.Z);
                            field.SetValue(attributeMonoBehaviors[key], val);
                        } else if (field.FieldType == typeof(Quaternion))
                        {
                            UnityEngine.Vector3 val =
                                new Vector3(
                                    (float)customData.NVector.X,
                                    (float)customData.NVector.Y,
                                    (float)customData.NVector.Z);
                            field.SetValue(attributeMonoBehaviors[key], Quaternion.Euler(val));
                        }
                        break;
                    default:
                        var value = HeliosUtility.FromTypedJson(customData.NCustom.ToByteArray());
                        // var value = HeliosUtility.ByteToObject(customData.NCustom);
                        // if (field.FieldType == typeof(Color))
                        // {
                        //     ColorUtility.TryParseHtmlString("#" + value, out Color loadedColor);
                        //     value = loadedColor;
                        // }
                        // else if (field.FieldType == typeof(Color32))
                        // {
                        //     ColorUtility.TryParseHtmlString("#" + value, out Color loadedColor);
                        //     value = (Color32)loadedColor;
                        // }
                        field.SetValue(attributeMonoBehaviors[key], value);
                        break;
                }
            initialHeliosValues[key] = field.GetValue(attributeMonoBehaviors[key]);
            if(heliosAttributeCallbacks.ContainsKey(key))
                {
                    CallMethodByName(heliosAttributeCallbacks[key].Item2, heliosAttributeCallbacks[key].Item1);
                }
            }
            
        }
        
        internal void CallMethodByName(HeliosMonoBehavior behavior, string methodName)
        {
            // Get the type of the current class
            Type type = behavior.GetType();

            // Get the method information using the method name
            var methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            // Check if the method exists and is not null
            if (methodInfo != null)
            {
                // Invoke the method on the current instance
                methodInfo.Invoke(behavior, null);
            }
            else
            {
                Debug.LogWarning("Method " + methodName + " not found in " + type);
            }
        }
    }
    
    public class DefaultPrefabPool : IHeliosPrefabPool
    {
        private readonly Dictionary<uint, GameObject> GOCache = new Dictionary<uint, GameObject>();
        private readonly Dictionary<uint, bool> BooleanCache = new Dictionary<uint, bool>(); 
        
        public GameObject Instantiate(uint prefabId, Vector3 position, Quaternion rotation)
        {
            GameObject go = null;
            bool cached = GOCache.TryGetValue(prefabId, out go);
            if (!cached)
            {
                go = HeliosNetwork.HeliosSettings.NetworkPrefabs.FindPrefabByNetworkId(prefabId).gameObject;
                if (go == null)
                {
                    Debug.LogError($"");
                }
                else
                {
                    GOCache.Add(prefabId, go);
                    BooleanCache.Add(prefabId, go.activeSelf);
                }
            }

            bool isActive = go.activeSelf;
            if(isActive) go.SetActive(false);

            GameObject instance = GameObject.Instantiate(go, position, rotation);
            SetActivePrefabPool(prefabId);
            instance.SetActive(false);
            
            return instance;
        }

        public void Destroy(GameObject gameObject)
        {
            // 내 것만 ?
            var RemovePkt = new C_REMOVE_NETWORK_OBJECTS();
            ObjectInfo objectInfo = new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = 0,
                    InstanceID = gameObject.GetComponentInSelfOrParent<HeliosObject>().InstanceId
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
            };
            RemovePkt.ObjectInfos.Add(objectInfo);
            HeliosNetwork.RaiseEvent(EventCode.PKT_C_REMOVE_NETWORK_OBJECTS, RemovePkt);
            HeliosNetwork.HeliosObjectList.RemoveAt((int)gameObject.GetComponentInSelfOrParent<HeliosObject>().InstanceId);
            GameObject.Destroy(gameObject);
        }
        
        public void Destroy(uint id)
        {
            List<HeliosMonoBehavior> des = new List<HeliosMonoBehavior>(HeliosNetwork.HeliosObjectList);
            var obj = des.Find(x => x.ObjectInfo.ObjectID.InstanceID == id);
            // if(obj == null) return;

            if (obj)
            {
                HeliosNetwork.RealtimeClient.OnObjectDestroyed(obj.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo);
                if (obj.IsMine)
                {
                    var RemovePkt = new C_REMOVE_NETWORK_OBJECTS();
                    ObjectInfo objectInfo = obj.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo;
                    RemovePkt.ObjectInfos.Add(objectInfo);
                    HeliosNetwork.RaiseEvent(EventCode.PKT_C_REMOVE_NETWORK_OBJECTS, RemovePkt);
                }
                HeliosNetwork.HeliosObjectList.Remove(obj);
                GameObject.Destroy(obj.gameObject);
            }
        }

        private void SetActivePrefabPool(uint prefabId)
        {
            var go = HeliosNetwork.HeliosSettings.NetworkPrefabs.FindPrefabByNetworkId(prefabId).gameObject;
            go.SetActive(BooleanCache[prefabId]);
        }

        public bool GetPrefabPoolActive(uint prefabId)
        {
            var go = HeliosNetwork.HeliosSettings.NetworkPrefabs.FindPrefabByNetworkId(prefabId).gameObject;
            return go.activeSelf;
        }
    }

    public abstract class MonoBehaviorHeliosCallbacks : HeliosMonoBehavior, IConnectionCallbacks, IMakingRoomCallbacks,
        IInRoomCallbacks, IMakingGroupCallbacks, IInGroupCallbacks, IOnEventCallbacks, IErrorInfoCallbacks
    {
        public virtual void OnEnable()
        {
            HeliosNetwork.AddCallbackTarget(this);
        }
        
        public virtual void OnDisable()
        {
            HeliosNetwork.RemoveCallbackTarget(this);
        }

        public abstract void OnConnectedToMasterServer();

        public abstract void OnConnected();

        public abstract void OnDisconnected();

        public abstract void OnCustomAuthenticationResponse(Dictionary<string, object> data);

        public abstract void OnCustomAuthenticationFailed(string debugMessage);

        public abstract void OnCreatedRoom();

        public abstract void OnCreatedRoomFailed(string message);

        public abstract void OnJoinedRoom();
        public abstract void OnJoinedRoomFailed(string message);

        public abstract void OnLeftRoom();

        public abstract void OnPlayerEnteredRoom(Player newPlayer);

        public abstract void OnPlayerLeftRoom(Player otherPlayer);

        public abstract void OnMasterClientSwitched(Player newMasterClient);
        public abstract void OnObjectInstantiated(ObjectInfo objectInfo);
        public abstract void OnObjectDestroyed(ObjectInfo objectInfo);

        public abstract void OnCreatedGroup();

        public abstract void OnCreatedGroupFailed(string message);

        public abstract void OnJoinedGroup();

        public abstract void OnJoinedGroupFailed(string message);

        public abstract void OnLeftGroup();

        public abstract void OnPlayerEnteredGroup(Player newPlayer);

        public abstract void OnPlayerLeftGroup(Player otherPlayer);

        public abstract void OnEvent(EventData eventData);

        public abstract void OnErrorInfo(string errorInfo);
    }
}
