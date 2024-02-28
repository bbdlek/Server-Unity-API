using System;
using System.Collections.Generic;
using System.Threading;
using MVS.Realtime;
using UnityEngine;

namespace MVS.Helios
{
    public class HeliosMonoBehavior : MonoBehaviour
    {
        
    }

    public class DefaultPrefabPool : IHeliosPrefabPool
    {
        public readonly Dictionary<string, GameObject> GOCache = new Dictionary<string, GameObject>();
        
        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            GameObject go = null;
            bool cached = GOCache.TryGetValue(prefabId, out go);
            if (!cached)
            {
                go = Resources.Load<GameObject>(prefabId);
                if (go == null)
                {
                    Debug.LogError($"");
                }
                else
                {
                    GOCache.Add(prefabId, go);
                }
            }

            bool isActive = go.activeSelf;
            if(isActive) go.SetActive(false);

            GameObject instance = GameObject.Instantiate(go, position, rotation);
            
            if(isActive) go.SetActive(true);
            return instance;
        }

        public void Destroy(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
        }
    }

    public class MonoBehaviorHeliosCallbacks : HeliosMonoBehavior, IConnectionCallbacks, IMakingRoomCallbacks,
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

        public virtual void OnConnected()
        {
        }

        public virtual void OnConnectedToMaster()
        {
        }

        public virtual void OnDisconnected()
        {
        }

        public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public virtual void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        public virtual void OnCreatedRoom()
        {
        }

        public virtual void OnCreatedRoomFailed(short failCode, string message)
        {
        }

        public virtual void OnJoinedRoom()
        {
        }

        public virtual void OnJoinedRoomFailed(short failCode, string message)
        {
        }

        public virtual void OnLeftRoom()
        {
        }

        public virtual void OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        public virtual void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public virtual void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public virtual void OnCreatedGroup()
        {
        }

        public virtual void OnCreatedGroupFailed(short failCode, string message)
        {
        }

        public virtual void OnJoinedGroup()
        {
        }

        public virtual void OnJoinedGroupFailed(short failCode, string message)
        {
        }

        public virtual void OnLeftGroup()
        {
        }

        public virtual void OnPlayerEnteredGroup(Player newPlayer)
        {
        }

        public virtual void OnPlayerLeftGroup(Player otherPlayer)
        {
        }

        public virtual void OnEvent(EventData eventData)
        {
        }

        public virtual void OnErrorInfo()
        {
        }
    }
}
