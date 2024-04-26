using System.Collections.Generic;
using Google.Protobuf;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;

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
            foreach (var ho in HeliosNetwork.HeliosObjectList.FindAll(x => x.hasUpdate))
            {
                var updateObject = new ObjectInfo();
                if (ho.GetComponent<HeliosObject>())
                {
                    var heliosObject = ho.GetComponent<HeliosObject>();
                    updateObject.ObjectID = new ObjectID
                    {
                        PrefabID = heliosObject.PrefabId,
                        InstanceID = heliosObject.InstanceId,
                    };
                    updateObject.SyncType = ObjectSyncType.PersonalOwn;
                    updateObject.OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId;
                }
                else
                {
                    updateObject = ho.ObjectInfo;
                }

                for (int i = 0; i < ho.heliosAttributes.Count; i++)
                {
                    HeliosVariable hv = new HeliosVariable();
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
                    }
                    ho.ObjectInfo.CustomData[i] = hv.ToByteString();
                    ho.initialHeliosValues[i] = ho.heliosAttributes[i].GetValue(ho.attributeMonoBehaviors[i]);
                }
                data.ObjectInfos.Add(updateObject);
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, data);
            }
        }

        public void OnConnected()
        {
            Debug.Log("OnConnected");
        }

        public void OnConnectedToMaster()
        {
            
        }

        public void OnDisconnected()
        {
            
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

        public void OnPlayerEnteredGroup(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftGroup(Player otherPlayer)
        {
            
        }
    }
}