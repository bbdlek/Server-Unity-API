using System.Collections.Generic;
using MVS.Realtime;
using UnityEngine;

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