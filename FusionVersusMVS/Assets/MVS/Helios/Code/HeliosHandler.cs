using System;
using System.Collections.Generic;
using System.Linq;
using _1_Scripts._8_HeliosTest;
using Google.Protobuf;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using UnityEngine.Profiling;
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
            }
        }

        private void CheckAndUpdateVariables()
        {
            var data = new C_UPDATE_NETWORK_OBJECTS();
            foreach (var ho in HeliosNetwork.HeliosObjectList.FindAll(x => x.hasUpdate))
            {
                Debug.Log("InstanceID :" + ho.ObjectInfo.ObjectID.InstanceID);
                var updateObject = new ObjectInfo
                {
                    ObjectID = ho.ObjectInfo.ObjectID,
                    SyncType = ho.ObjectInfo.SyncType,
                    OwnerPlayerID = ho.ObjectInfo.OwnerPlayerID,
                };
                foreach (var variable in ho.HeliosVariableTable)
                {
                    Debug.Log(variable.GetValue().NInt32);
                    updateObject.CustomData.Add(variable.GetValue().ToByteString());
                    Debug.Log(HeliosVariable.Parser.ParseFrom(variable.GetValue().ToByteString()).NInt32);
                    variable.SetFlag(false);
                }
                data.ObjectInfos.Add(updateObject);
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, data);
                ho.hasUpdate = false;
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