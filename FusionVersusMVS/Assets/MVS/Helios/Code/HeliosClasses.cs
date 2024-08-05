using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVS.Helios.Utility;
using MVS.Realtime;
using Protocol;
using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public class HeliosMonoBehavior : MonoBehaviour
    {
        public bool hasUpdate
        {
            get
            {
                bool b = false;
                if (heliosAttributes.Count == 0) return false;
                for (int i = CustomVariablesUnity.ScaleKey + 1; i < heliosAttributes.Count + CustomVariablesUnity.ScaleKey + 1; i++)
                {
                    if (HeliosUtility.IsListType(heliosAttributes[i].FieldType))
                    {
                        b = !HeliosUtility.CheckListEquals(heliosAttributes[i].GetValue(attributeMonoBehaviors[i]),
                            initialHeliosValues[i]);
                    }
                    else if (HeliosUtility.IsDictionaryType(heliosAttributes[i].FieldType))
                    {
                        b = !HeliosUtility.CheckDictionariesEqual(heliosAttributes[i].GetValue(attributeMonoBehaviors[i]),
                            initialHeliosValues[i]);
                    }
                    else 
                    {
                        try
                        {
                            if (!heliosAttributes[i].GetValue(attributeMonoBehaviors[i]).Equals(initialHeliosValues[i]))
                                b = true;
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                }
                return b;
            }
        }

        public Dictionary<int, FieldInfo> heliosAttributes = new Dictionary<int, FieldInfo>();

        public Dictionary<int, Tuple<string, HeliosMonoBehavior>> heliosAttributeCallbacks =
            new Dictionary<int, Tuple<string, HeliosMonoBehavior>>();
        public Dictionary<int, HeliosMonoBehavior> attributeMonoBehaviors = new Dictionary<int, HeliosMonoBehavior>();
        public Dictionary<int, object> initialHeliosValues = new Dictionary<int, object>();

        public Dictionary<ulong, Tuple<MethodInfo, HeliosMonoBehavior>> RPCMethods =
            new Dictionary<ulong, Tuple<MethodInfo, HeliosMonoBehavior>>();
        

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
                if (GetComponent<HeliosObject>())
                {
                    return GetComponent<HeliosObject>().IsMine;
                }
                return false;
            }
        }
        
        public virtual void Awake()
        {
            if(ObjectInfo == null)
                ObjectInfo = new ObjectInfo();
            
            FindNetworkedVariables();
            FindRPCMethods();
            
            if(!GetComponentInChildren<HeliosObject>())
            {
                HeliosNetwork.HeliosObjectList.Add(this);
                ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(this);
            }
        }

        public void RPC(string methodName, ulong[] targetPlayerIDs = null, params object[] args)
        {
            ObjectID objectID;
            if (GetComponent<HeliosObject>())
            {
                objectID = GetComponent<HeliosObject>().ObjectInfo.ObjectID;
            }
            else
            {
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
            var method = RPCMethods[methodNameHash].Item1;
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

        private bool _isFindNetworkedVariables = false;
        private bool _isFindRPC = false;
        
        public void FindNetworkedVariables()
        {
            if(_isFindNetworkedVariables) return;
            _isFindNetworkedVariables = true;
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
                        hv.NCustom = HeliosUtility.ObjectToBytes2(obj);

                        // hv.NString = HeliosUtility.ObjectToString(obj);


                        // if (field.FieldType.IsSerializable)
                        // {
                        //     hv.NCustom = HeliosUtility.ObjectToBytes(obj);   
                        // }
                        // else
                        // {
                        //     if (field.FieldType == typeof(Color))
                        //     {
                        //         var objColor = ColorUtility.ToHtmlStringRGBA((Color)obj);
                        //         hv.NCustom = HeliosUtility.ObjectToBytes(objColor); 
                        //     } 
                        //     else if (field.FieldType == typeof(Color32))
                        //     {
                        //         var objColor32 = ColorUtility.ToHtmlStringRGBA((Color32)obj);
                        //         hv.NCustom = HeliosUtility.ObjectToBytes(objColor32); 
                        //     }
                        // }
                    }
                    if (GetComponent<HeliosObject>())
                    {
                        var ho = GetComponent<HeliosObject>();
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
                            ho.ObjectInfo.Values.Add(hv);
                        
                        //ATTRIBUTE
                        if(!ho.heliosAttributes.ContainsKey(key))
                            ho.heliosAttributes.Add(key, field);
                        
                        OnChangedAttribute callbackAttribute =
                            (OnChangedAttribute)Attribute.GetCustomAttribute(field, typeof(OnChangedAttribute));
                        if (callbackAttribute != null)
                        {
                            if(!ho.heliosAttributeCallbacks.ContainsKey(key))
                                ho.heliosAttributeCallbacks.Add(key, Tuple.Create(callbackAttribute.MethodName, this));
                        }
                        
                        if(!ho.attributeMonoBehaviors.ContainsKey(key))
                            ho.attributeMonoBehaviors.Add(key, this);
                        
                        if(!ho.initialHeliosValues.ContainsKey(key))
                            ho.initialHeliosValues.Add(key, DeepCopyHelper.DeepCopy(obj));
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
                        if(!heliosAttributes.ContainsKey(key))
                            heliosAttributes.Add(key, field);
                        
                        OnChangedAttribute callbackAttribute =
                            (OnChangedAttribute)Attribute.GetCustomAttribute(field, typeof(OnChangedAttribute));
                        if (callbackAttribute != null)
                        {
                            if(!heliosAttributeCallbacks.ContainsKey(key))
                                heliosAttributeCallbacks.Add(key, Tuple.Create(callbackAttribute.MethodName, this));
                        }
                        
                        if(!attributeMonoBehaviors.ContainsKey(key))
                            attributeMonoBehaviors.Add(key, this);
                        
                        if(!initialHeliosValues.ContainsKey(key))
                            initialHeliosValues.Add(key, DeepCopyHelper.DeepCopy(obj));    
                        
                    }
                }
            }
        }
        

        public void FindRPCMethods()
        {
            if(_isFindRPC) return;
            _isFindRPC = true;
            Type classType = GetType();
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                HeliosRPCAttribute attribute =
                    (HeliosRPCAttribute)Attribute.GetCustomAttribute(method, typeof(HeliosRPCAttribute));
                
                if (attribute != null)
                {
                    
                    if (GetComponent<HeliosObject>())
                    {
                        string methodName = method.Name;
                        var hash = HeliosUtility.Compute64BitHash(methodName);
                        
                        GetComponent<HeliosObject>().RPCMethods.Add(hash, Tuple.Create(method, this));
                    }
                    else
                    {
                        string methodName = method.Name;
                        var hash = HeliosUtility.Compute64BitHash(methodName);
                       
                        RPCMethods.Add(hash, Tuple.Create(method, this));
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
                var field = heliosAttributes[key];
                Debug.Log(field.Name);
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
                        Debug.Log(customData.NCustom);
                        var value = HeliosUtility.BytesToObject2(customData.NCustom); 
                        field.SetValue(attributeMonoBehaviors[key], value);
                        break;
                }
            initialHeliosValues[key] = field.GetValue(attributeMonoBehaviors[key]);
            if(heliosAttributeCallbacks.ContainsKey(key))
                CallMethodByName(heliosAttributeCallbacks[key].Item2, heliosAttributeCallbacks[key].Item1);
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
                    InstanceID = gameObject.GetComponent<HeliosObject>().InstanceId
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
            };
            RemovePkt.ObjectInfos.Add(objectInfo);
            HeliosNetwork.RaiseEvent(EventCode.PKT_C_REMOVE_NETWORK_OBJECTS, RemovePkt);
            HeliosNetwork.HeliosObjectList.RemoveAt((int)gameObject.GetComponent<HeliosObject>().InstanceId);
            GameObject.Destroy(gameObject);
        }
        
        public void Destroy(uint id)
        {
            List<HeliosMonoBehavior> des = new List<HeliosMonoBehavior>(HeliosNetwork.HeliosObjectList);
            var obj = des.Find(x => x.ObjectInfo.ObjectID.InstanceID == id);
            // if(obj == null) return;

            if (obj)
            {
                HeliosNetwork.RealtimeClient.OnObjectDestroyed(obj.GetComponent<HeliosObject>().ObjectInfo);
                if (obj.IsMine)
                {
                    var RemovePkt = new C_REMOVE_NETWORK_OBJECTS();
                    ObjectInfo objectInfo = obj.GetComponent<HeliosObject>().ObjectInfo;
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
