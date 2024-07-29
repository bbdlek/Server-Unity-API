using System;
using System.Collections.Generic;
using MVS.Helios.Utility;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using Vector3 = Protocol.Vector3;

namespace MVS.Helios
{
    public class HeliosHandler : ConnectionHandler, IConnectionCallbacks, IInRoomCallbacks, IInGroupCallbacks
    {
        private static HeliosHandler instance;

        public static HeliosHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<HeliosHandler>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "HeliosHandler";
                        instance = go.AddComponent<HeliosHandler>();
                    }
                }

                return instance;
            }
        }


        private void Awake()
        {
            if (instance == null || ReferenceEquals(this, instance))
            {
                instance = this;
                base.Awake();
            }
            else
            {
                Destroy(this);
            }
        }
        
        private float pingInterval = 5.0f; // 5초마다 Ping 메시지 전송
        private float timeSinceLastPing = 0.0f;

        private void SendPing()
        {
            HeliosNetwork.RealtimeClient.OpSendHeartBeat();
        }

        private void Update()
        {
            if (HeliosNetwork.IsConnected)
            {
                timeSinceLastPing += Time.deltaTime;
                if (timeSinceLastPing >= pingInterval)
                {
                    SendPing();
                    timeSinceLastPing = 0.0f;
                }
            }
        }

        protected void FixedUpdate()
        {
            if (Time.timeScale > HeliosNetwork.MinimalTimeScaleToDispatchInFixedUpdate)
            {
                if(!HeliosNetwork.IsConnected) return;
                HeliosNetwork.RealtimeClient.RealtimePeer.ProcessIncomingData();
            }
        }
        
        private float _elapsedTime = 0f;

        protected void LateUpdate()
        {
            if(!HeliosNetwork.InGroup) return;
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= 1f / HeliosNetwork.SendRate)
            {
                CheckAndUpdateVariables();
                _elapsedTime = 0f;
            }
        }

        private void CheckAndUpdateVariables()
        {
            var data = new C_UPDATE_NETWORK_OBJECTS();
            foreach (var ho in HeliosNetwork.HeliosObjectList.FindAll(x => x.hasUpdate && (x.IsMine|| (x.ObjectInfo.SyncType == ObjectSyncType.GroupOwn && HeliosNetwork.CurrentGroup.IsLocalGroupOwner))))
            {
                ObjectInfo updateObject = new ObjectInfo();
                if (GetComponent<HeliosObject>())
                {
                    updateObject.ObjectID = GetComponent<HeliosObject>().ObjectInfo.ObjectID;
                    updateObject.SyncType = GetComponent<HeliosObject>().ObjectInfo.SyncType;
                    updateObject.OwnerPlayerID = GetComponent<HeliosObject>().ObjectInfo.OwnerPlayerID;
                    for (int var = 0; var < 3; var++)
                    {
                        updateObject.Values.Add(GetComponent<HeliosObject>().ObjectInfo.Values[var]);
                    }
                }
                else
                {
                    updateObject.ObjectID = ho.ObjectInfo.ObjectID;
                    updateObject.SyncType = ho.ObjectInfo.SyncType;
                    updateObject.OwnerPlayerID = ho.ObjectInfo.OwnerPlayerID;
                }
                // var updateObject = new ObjectInfo();

                for (int i = CustomVariablesUnity.ScaleKey + 1; i < ho.heliosAttributes.Count + CustomVariablesUnity.ScaleKey + 1; i++)
                {
                    if (HeliosUtility.IsListType(ho.heliosAttributes[i].FieldType))
                    {
                        bool listEquals = HeliosUtility.CheckListEquals(ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]),
                            ho.initialHeliosValues[i]);
                        if (listEquals)
                            continue;
                    }
                    else
                    {
                        if (ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]).Equals(ho.initialHeliosValues[i]))
                            continue;
                    }
                    HeliosVariable hv = new HeliosVariable();
                    hv.Key = ho.ObjectInfo.Values[i].Key;
                    switch (ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]))
                    {
                        case int value:
                            hv.NInt32 = value;
                            break;
                        case long value:
                            hv.NInt64 = value;
                            break;
                        case float value:
                            hv.NFloat = value;
                            break;
                        case double value:
                            hv.NDouble = value;
                            break;
                        case bool value:
                            hv.NBool = value;
                            break;
                        case string value:
                            hv.NString = value;
                            break;
                        case Vector2 value:
                            hv.NVector = new Vector3
                            {
                                X = value.x,
                                Y = value.y,
                                Z = 0
                            };
                            break;
                        case UnityEngine.Vector3 value:
                            hv.NVector = new Vector3
                            {
                                X = value.x,
                                Y = value.y,
                                Z = value.z
                            };
                            break;
                        case UnityEngine.Quaternion value:
                            UnityEngine.Vector3 val = value.eulerAngles;
                            hv.NVector = new Vector3
                            {
                                X = val.x,
                                Y = val.y,
                                Z = val.z
                            };
                            break;
                        default:
                            if (ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]) is Color)
                            {
                                hv.NCustom = HeliosUtility.ObjectToBytes(ColorUtility.ToHtmlStringRGBA((Color)ho
                                    .heliosAttributes[i]
                                    .GetValue(ho.attributeMonoBehaviors[i])));
                            }
                            else if (ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]) is Color32)
                            {
                                hv.NCustom = HeliosUtility.ObjectToBytes(ColorUtility.ToHtmlStringRGBA((Color32)ho
                                    .heliosAttributes[i]
                                    .GetValue(ho.attributeMonoBehaviors[i])));
                            }
                            else
                            {
                                hv.NCustom =
                                    HeliosUtility.ObjectToBytes(ho.heliosAttributes[i]
                                        .GetValue(ho.attributeMonoBehaviors[i]));    
                            }
                            
                            break;
                    }
                    ho.ObjectInfo.Values[i] = hv;
                    if (HeliosUtility.IsListType(ho.initialHeliosValues[i].GetType()))
                    {
                        ho.initialHeliosValues[i] =
                            DeepCopyHelper.DeepCopy(ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]));
                    }
                    else ho.initialHeliosValues[i] = ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]);
                    updateObject.Values.Add(hv);

                    if(ho.heliosAttributeCallbacks.ContainsKey(i))
                        ho.CallMethodByName(ho.heliosAttributeCallbacks[i].Item2, ho.heliosAttributeCallbacks[i].Item1);
                }
                // data.ObjectInfos.Add(ho.ObjectInfo);
                data.ObjectInfos.Add(updateObject);
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, data);
            }
        }

        public void OnConnectedToMasterServer()
        {
            
        }

        public void OnConnected()
        {
            
        }

        public void OnDisconnected()
        {
            HeliosNetwork.RemoveMyObjects();
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            
        }

        public void OnObjectInstantiated(ObjectInfo objectInfo)
        {
            
        }

        public void OnObjectDestroyed(ObjectInfo objectInfo)
        {
            
        }

        public void OnPlayerEnteredGroup(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftGroup(Player otherPlayer)
        {
            
        }
    }
}