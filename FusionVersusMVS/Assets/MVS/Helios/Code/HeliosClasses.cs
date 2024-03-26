using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using _1_Scripts._8_HeliosTest;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = MVS.Realtime.HeliosVariable;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public class HeliosMonoBehavior : MonoBehaviour
    {
        public List<HeliosVariable> HeliosVariableTable = new List<HeliosVariable>();
        
        protected bool isMine = false;
        
        public bool IsMine
        {
            get { return isMine; }
            set { isMine = value; }
        }

        public virtual void Awake()
        {
            Debug.Log("MONO");
            if(!GetComponentInChildren<HeliosObject>())
            {
                FindHeliosVariable();
                SetHeliosVariableIndex();
            }
        }

        public void FindHeliosVariable()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (typeof(HeliosVariable).IsAssignableFrom(field.FieldType))
                {
                    HeliosVariable heliosVariable = (HeliosVariable)field.GetValue(this);
                    HeliosVariableTable.Add(heliosVariable);
                }
            }
        }

        private int idx = 0;

        public void SetHeliosVariableIndex()
        {
            Debug.Log("SetVIndex");
            if (GetComponent<HeliosObject>())
            {
                Debug.Log("isHO");
                Debug.Log(HeliosVariableTable.Count);
                foreach (var heliosVariable in HeliosVariableTable)
                {
                    HeliosObject ho = GetComponent<HeliosObject>();
                    heliosVariable.SetIndex((int)(ho.instanceId * 100000 + idx));
                    Debug.Log(heliosVariable.Index);
                    HeliosNetwork.HeliosVariableDic.Add((int)(ho.instanceId * 100000 + idx), heliosVariable);
                    idx++;
                }
            }
            else
            {
                Debug.Log("!isHO");
                foreach (var heliosVariable in HeliosVariableTable)
                {
                    heliosVariable.SetIndex(idx);
                    HeliosNetwork.HeliosVariableDic.Add(idx, heliosVariable);
                    idx++;
                }
            }
        }
    }

    public class DefaultPrefabPool : IHeliosPrefabPool
    {
        public readonly Dictionary<uint, GameObject> GOCache = new Dictionary<uint, GameObject>();
        
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
            // 내 것만 ?
            var RemovePkt = new C_REMOVE_NETWORK_OBJECTS();
            ObjectInfo objectInfo = new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = 0,
                    InstanceID = gameObject.GetComponent<HeliosObject>().instanceId
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
            };
            RemovePkt.ObjectInfos.Add(objectInfo);
            HeliosNetwork.RaiseEvent(EventCode.PKT_C_REMOVE_NETWORK_OBJECTS, RemovePkt);
            HeliosNetwork.HeliosObjectList.Remove(gameObject.GetComponent<HeliosObject>().instanceId);
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
            HeliosNetwork.RemoveMyObjects();
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
