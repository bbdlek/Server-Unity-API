using System;
using System.Collections.Generic;
using System.Linq;
using _1_Scripts._8_HeliosTest;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using UnityEngine.Profiling;
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
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= 1f / HeliosNetwork.SendRate)
            {
                CheckAndUpdateVariables();
            }
        }

        private void CheckAndUpdateVariables()
        {
            foreach (var pair in HeliosNetwork.HeliosVariableDic)
            {
                var variable = pair.Value;
                Debug.Log($"{pair.Key}key, {pair.Value.Index} idx, {variable.IsUpdate}");
                if (variable.IsUpdate)
                {
                    //Send
                    Debug.Log($"SEND VARIABLE {variable.Index}");
                    var data = new C_VARIABLE
                    {
                        Index = (ulong)HeliosNetwork.HeliosVariableDic.FirstOrDefault(x => x.Value == variable).Key,
                        HeliosVariable = variable.GetValue()
                    };
                    HeliosNetwork.RaiseEvent(CustomEventCode.Variable, data);
                    variable.SetFlag(false);
                }
            }
            //
            // foreach (var variable in HeliosNetwork.HeliosVariables)
            // {
            //     if (variable.IsUpdate)
            //     {
            //         //Send
            //         Debug.Log("SEND VARIABLE");
            //         var data = new C_VARIABLE
            //         {
            //             Index = (ulong)HeliosNetwork.HeliosVariables.IndexOf(variable),
            //             HeliosVariable = variable.GetValue()
            //         };
            //         HeliosNetwork.RaiseEvent(CustomEventCode.Variable, data);
            //         variable.SetFlag(false);
            //     }
            // }
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