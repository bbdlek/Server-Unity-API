
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using MVS.Realtime;
using Protocol;
using UnityEditor;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public static partial class HeliosNetwork
    {
        // Version Of Helios
        public const string HeliosVersion = "0.0.1";
        
        // AppVersion
        private static string _appVersion;

        public static string AppVersion
        {
            get => _appVersion;
            set
            {
                _appVersion = value;
                RealtimeClient.AppVersion = $"{value}_{HeliosVersion}";
            }
        }
        
        // Realtime Client
        public static RealtimeClient RealtimeClient;
        
        // Limit Users
        public static readonly int MAX_USERS = 1000;
        
        // 
        private const string HeliosSettingsFileName = "HeliosServerSettings";

        private static HeliosSettings _heliosSettings;

        public static HeliosSettings HeliosSettings
        {
            get
            {
                if (_heliosSettings == null)
                {
                    LoadOrCreateSettings();
                }

                return _heliosSettings;
            }
            private set => _heliosSettings = value;
        }

        public static string ServerAddress => (RealtimeClient != null) ? RealtimeClient.MVSAddress : "Not Connected";
        
        // public static string Region => (RealtimeClient != null) ? RealtimeClient.Region : "Not Connected";

        public static bool IsConnected
        {
            get
            {
                if (RealtimeClient == null)
                {
                    return false;
                }

                return RealtimeClient.IsConnected;
            }
        }
        
        public static bool IsConnectedAndReady
        {
            get
            {
                if (RealtimeClient == null)
                {
                    return false;
                }

                return RealtimeClient.IsConnectedAndReady;
            }
        }

        public static ClientState ClientState => RealtimeClient == null ? ClientState.DisConnected : RealtimeClient.State;

        public static ServerConnection ServerConnection => RealtimeClient == null ? ServerConnection.NameServer : RealtimeClient.Server;
        
        //Auth
        
        public static int SendRate
        {
            get
            {
                return 1000 / sendFrequency;
            }

            set
            {
                sendFrequency = 1000 / value;
            }
        }

        private static int sendFrequency = 33; // in milliseconds.
        
        public static bool IsMessageQueueRunning
        {
            get
            {
                return isMessageQueueRunning;
            }

            set
            {
                isMessageQueueRunning = value;
            }
        }

        /// <summary>Backup for property IsMessageQueueRunning.</summary>
        private static bool isMessageQueueRunning = true;
        
        public static float MinimalTimeScaleToDispatchInFixedUpdate = -1f;

        public static Room CurrentRoom => RealtimeClient == null ? null : RealtimeClient.CurrentRoom;
        
        public static Group CurrentGroup => RealtimeClient == null ? null : RealtimeClient.CurrentGroup;

        public static Player LocalPlayer => RealtimeClient == null ? null : RealtimeClient.LocalPlayer;

        public static List<Player> PlayerList
        {
            get
            {
                Room room = CurrentRoom;
                if (room != null)
                {
                    return new List<Player>(room.PlayerList.Values.OrderBy(x => x.UserId));
                }

                return new List<Player>();
            }
        }

        public static List<Player> OtherPlayerList
        {
            get
            {
                Room room = CurrentRoom;
                if (room != null)
                {
                    return new List<Player>(room.PlayerList.Values.OrderBy(x => x.UserId).Where(x => !x.IsLocal));
                }

                return new List<Player>();
            }
        }
        
        // TODO : SyncScene??

        public static bool InRoom => RealtimeClient.InRoom;
        
        public static bool InGroup => RealtimeClient.InGroup;
        
        // TODO : SendRate??
        
        // TODO : NetworkTime??
        
        // TODO : Background Alive??

        public static bool IsMasterClient =>
            RealtimeClient.CurrentRoom != null &&
            RealtimeClient.CurrentRoom.MasterClientId == LocalPlayer.UserId;
        
        // TODO : Count Of Players, Rooms, Groups ETC
        

        static HeliosNetwork()
        {
            if (RealtimeClient == null)
            {
                RealtimeClient = new RealtimeClient();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void StaticReInitialize()
        {
            if(!EditorApplication.isPlayingOrWillChangePlaymode) return;

            ConnectionProtocol protocol = HeliosSettings.AppSettings.Protocol;
            RealtimeClient = new RealtimeClient(protocol);

            RealtimeClient.EventReceived -= OnEvent;
            RealtimeClient.EventReceived += OnEvent;
            RealtimeClient.OpResponseReceived -= OnOperation;
            RealtimeClient.OpResponseReceived += OnOperation;
            RealtimeClient.StateChanged -= OnClientStateChanged;
            RealtimeClient.StateChanged += OnClientStateChanged;

            HeliosHandler.Instance.Client = RealtimeClient;

            Application.runInBackground = HeliosSettings.RunInBackground;
            
            // TODO : PrefabPool
            PrefabPool = new DefaultPrefabPool();

            // TODO : Register CustomType?

        }
        
        public static bool ConnectUsingSettings()
        {
            if (HeliosSettings == null)
            {
                Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: " + HeliosSettingsFileName);
                return false;
            }

            return ConnectUsingSettings(HeliosSettings.AppSettings);
        }

        public static bool ConnectUsingSettings(AppSettings appSettings)
        {
            if (RealtimeClient.RealtimePeer.PeerState != PeerState.Disconnected)
            {
                Debug.LogWarning($"ConnectUsingSettings() Failed Because State. Current State : {RealtimeClient.RealtimePeer.PeerState}");
                return false;
            }

            if (HeliosSettings == null)
            {
                Debug.LogError("There is no App Settings");
                return false;
            }

            RealtimeClient.RealtimePeer.TransportProtocol = appSettings.Protocol;
            RealtimeClient.ConnectionProtocol = null;
            // RealtimeClient.AuthMode = appSettings.;

            IsMessageQueueRunning = true;
            RealtimeClient.AppId = appSettings.AppId;
            AppVersion = appSettings.AppVersion;

            // TODO : Master, Name Server
            return RealtimeClient.Connect(appSettings.Server, appSettings.Port.ToString(), appSettings.AppId,
                ServerConnection.MVS);
        }

        public static void Disconnect()
        {
            if(RealtimeClient == null) return;
            
            RealtimeClient.Disconnect();
        }
        
        // TODO : Reconnect
        public static bool Reconnect()
        {
            return true;
        }
        
        // TODO : KickPlayer
        public static bool KickPlayer(Player kickPlayer)
        {
            return true;
        }

        public static bool JoinOrCreateRoom(string AuthToken, long AppID, long WaplRoomID, string Name)
        {
            // if (!IsConnectedAndReady) return false;
            JoinRoomParams opParams = new JoinRoomParams
            {
                AuthToken = AuthToken,
                AppID = AppID,
                RoomID = WaplRoomID,
                Name = Name
            };

            return RealtimeClient.OpCreateRoom(opParams);
        }

        public static bool JoinGroup(uint sceneNumber, uint channelID)
        {
            // if (!IsConnectedAndReady) return false;

            return RealtimeClient.OpJoinGroup(sceneNumber, channelID);
        }

        public static bool RaiseEvent(EventCode eventCode, IMessage pkt)
        {
            if (!InGroup) return false;

            return RealtimeClient.OpRaiseEvent(eventCode, pkt);
        }

        #region Instantiate

        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (CurrentRoom == null)
                return null;
            var comp = prefab.GetComponent<HeliosObject>();
            if (comp != null)
                return Instantiate(comp.prefabId, position, rotation);
            else
            {
                Debug.LogError($"{prefab.name} does not have HeliosObject Component");
                return null;
            }

        }

        public static GameObject Instantiate(uint prefabId, Vector3 position, Quaternion rotation)
        {
            if (CurrentRoom == null)
                return null;

            InstantiateParams instantiateParams = new InstantiateParams
            {
                prefabId = prefabId,
                position = position,
                rotation = rotation,
                creator = LocalPlayer
            };
            
            return NetworkInstantiate(instantiateParams, false);
        }

        private static GameObject NetworkInstantiate(ObjectInfo objectInfo)
        {
            Debug.Log(objectInfo.NumberProps.Count);
            var propPos = objectInfo.NumberProps.FirstOrDefault(prop => prop.Index == PropsID.Position3D).Value;
            var propRot = objectInfo.NumberProps.FirstOrDefault(prop => prop.Index == PropsID.Rotation3D).Value;
            Vector3 position = new Vector3((float)propPos[0], (float)propPos[1], (float)propPos[2]);
            // Quaternion rotation = new Quaternion((float)propRot[0], (float)propRot[1], (float)propRot[2], (float)propRot[3]);
            InstantiateParams instantiateParams = new InstantiateParams
            {
                prefabId = objectInfo.ObjectID.PrefabID,
                instanceId = objectInfo.ObjectID.InstanceID,
                position = position,
                rotation = Quaternion.identity,
                creator = CurrentRoom.GetPlayer(objectInfo.OwnerPlayerID)
            };
            return NetworkInstantiate(instantiateParams, false, true);
        }

        private static GameObject NetworkInstantiate(InstantiateParams instantiateParams, bool isRoomObject = false,
            bool instantiateEvent = false)
        {
            GameObject go = null;
            
            go = _prefabPool.Instantiate(instantiateParams.prefabId, instantiateParams.position,
                instantiateParams.rotation);

            if (go == null)
            {
                return null;
            }

            if (go.activeSelf)
            {
                
            }

            bool isLocalInstantiate = !instantiateEvent && LocalPlayer.Equals(instantiateParams.creator);
            Debug.Log(isLocalInstantiate);
            
            // TODO : IF Local Instantiate
            if (isLocalInstantiate)
            {
                Debug.Log(instantiateParams.instanceId);
                go.GetComponent<HeliosTransform>().IsMine = true;
                SendInstantiate(instantiateParams, isRoomObject);
                MyHeliosObjectQueue.Enqueue(go.GetComponent<HeliosObject>());
            }
            else
            {
                Debug.Log(instantiateParams.instanceId);
                go.GetComponent<HeliosTransform>().IsMine = false;
                go.GetComponent<HeliosObject>().instanceId = instantiateParams.instanceId;
                HeliosObjectList.Add(instantiateParams.instanceId, go.GetComponent<HeliosObject>());
            }
            
            go.SetActive(true);
            
            
            return go;
        }

        internal static bool SendInstantiate(InstantiateParams instantiateParams, bool isRoomObject = false)
        {
            var pkt = new C_ADD_NETWORK_OBJECTS();
            var ObjectInfo = new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = instantiateParams.prefabId,
                    InstanceID = instantiateParams.instanceId,
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = LocalPlayer.UserId,
            };
            ObjectInfo.NumberProps.Add(new CustomNumberProp
            {
                Index = PropsID.Position3D,
                Value = { instantiateParams.position.x, instantiateParams.position.y, instantiateParams.position.z }
            });
            ObjectInfo.NumberProps.Add(new CustomNumberProp
            {
                Index = PropsID.Rotation3D,
                Value = { instantiateParams.rotation.x, instantiateParams.rotation.y, instantiateParams.rotation.z, instantiateParams.rotation.w }
            });
            pkt.ObjectInfos.Add(ObjectInfo);
            return SendEventInternal(EventCode.PKT_C_ADD_NETWORK_OBJECTS, pkt);
        }

        #endregion

        private static void NetworkUpdateObject(uint id, ObjectInfo objectInfo)
        {
            var propPos = objectInfo.NumberProps.FirstOrDefault(prop => prop.Index == PropsID.Position3D).Value;
            var propRot = objectInfo.NumberProps.FirstOrDefault(prop => prop.Index == PropsID.Rotation3D).Value;
            Vector3 position = new Vector3((float)propPos[0], (float)propPos[1], (float)propPos[2]);
            Quaternion rotation = new Quaternion((float)propRot[0], (float)propRot[1], (float)propRot[2], (float)propRot[3]);
            HeliosObjectList[id].heliosTransform.networkPosition = position;
            HeliosObjectList[id].heliosTransform.networkRotation = rotation;
        }

        private static void NetworkRemoveObject(uint id)
        {
            if(HeliosObjectList.TryGetValue(id, out var obj))
            {
                GameObject.Destroy(obj.gameObject);
                HeliosObjectList.Remove(id);
            }
        }
        
        private static bool SendEventInternal(EventCode eventCode, IMessage data)
        {
            // if (!InRoom)
            // {
            //     Debug.LogWarning("RaiseEvent(" + eventCode + ") failed. You are not in a Room");
            //     return false;
            // }

            return RealtimeClient.OpRaiseEvent(eventCode, data);
        }
        

        public static void LoadOrCreateSettings()
        {
            HeliosSettings = (HeliosSettings)Resources.Load(HeliosSettingsFileName, typeof(HeliosSettings));
        }
        
    }
}
