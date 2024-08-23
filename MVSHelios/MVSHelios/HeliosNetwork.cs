using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using MVS.Helios.Utility;
using MVS.Realtime;
using Protocol;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using WebSocketSharp;
using EventCode = MVS.Realtime.EventCode;
using RoomInfo = MVS.Realtime.RoomInfo;
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

        public static bool IsConnectedToMaster = false;

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

        public static float SendRate = 33.0f;
        
        public static float MinimalTimeScaleToDispatchInFixedUpdate = -1f;

        // public static List<Room> RoomList => RealtimeClient.MvsRoomInfos;

        public static Room CurrentRoom => RealtimeClient == null ? null : RealtimeClient.CurrentRoom;

        public static List<Group> GroupList => RealtimeClient == null ? null : CurrentRoom.GroupList;
        
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
        
        // TODO : SyncScene??

        public static bool InRoom => RealtimeClient.InRoom;
        
        public static bool InGroup => RealtimeClient.InGroup;
        
        // TODO : SendRate??
        
        // TODO : NetworkTime??
        
        // TODO : Background Alive??

        public static bool IsMasterClient =>
            RealtimeClient.CurrentGroup?.IsLocalGroupOwner ?? false;
        
        // TODO : Count Of Players, Rooms, Groups ETC
        

        static HeliosNetwork()
        {
#if !UNITY_EDITOR
            StaticReInitialize();
#else
            if(RealtimeClient == null)
                RealtimeClient = new RealtimeClient(HeliosSettings.AppSettings.Protocol);
#endif
        }

        #if UNITY_EDITOR && UNITY_2019_4_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        #endif
        private static void StaticReInitialize()
        {
            #if UNITY_EDITOR
            if(!EditorApplication.isPlayingOrWillChangePlaymode) return;
            #endif
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            HeliosSettings.AppSettings.Protocol = ConnectionProtocol.WebSocket;
            #endif
            
            ConnectionProtocol protocol = HeliosSettings.AppSettings.Protocol;
            if(RealtimeClient == null)
                RealtimeClient = new RealtimeClient(protocol);
            
            RealtimeClient.AppId = HeliosSettings.AppSettings.AppId;
            RealtimeClient.MVSDebug(DebugLevel.INFO, HeliosSettings.AppSettings.AppId);
            RealtimeClient.AppVersion = HeliosSettings.AppSettings.AppVersion;
            RealtimeClient.AppSettingsDebug = HeliosSettings.AppSettings.DebugLevel;
            RealtimeClient.IsUsingNameServer = HeliosSettings.AppSettings.IsUsingNameServer;
            RealtimeClient.NameServerAddress = HeliosSettings.AppSettings.NameServer;
            RealtimeClient.MasterServerAddress = HeliosSettings.AppSettings.MVM;
            RealtimeClient.RealtimePeer.TransportProtocol = HeliosSettings.AppSettings.Protocol;
            RealtimeClient.ConnectionProtocol = HeliosSettings.AppSettings.Protocol;
            
            RealtimeClient.EventReceived -= OnEvent;
            RealtimeClient.EventReceived += OnEvent;
            RealtimeClient.OpResponseReceived -= OnOperation;
            RealtimeClient.OpResponseReceived += OnOperation;
            RealtimeClient.StateChanged -= OnClientStateChanged;
            RealtimeClient.StateChanged += OnClientStateChanged;
            
            HeliosHandler.Instance.Client = RealtimeClient;
            
            Application.runInBackground = HeliosSettings.RunInBackground;
            SendRate = HeliosSettings.SendRate;
            
            // TODO : PrefabPool
            PrefabPool = new DefaultPrefabPool();
            
            // TODO : Register CustomType?
            CustomVariablesUnity.Register();

        }
        
        // public static async Task<bool> ConnectUsingSettings()
        // {
        //     if (HeliosSettings == null)
        //     {
        //         Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: " + HeliosSettingsFileName);
        //         return false;
        //     }
        //
        //     return await ConnectUsingSettings(HeliosSettings.AppSettings);
        // }

        private static bool ConnectUsingSettings(AppSettings appSettings)
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
            RealtimeClient.ConnectionProtocol = appSettings.Protocol;
            // RealtimeClient.AuthMode = appSettings.;
            
            RealtimeClient.AppId = appSettings.AppId;
            AppVersion = appSettings.AppVersion;

            RealtimeClient.AppSettingsDebug = appSettings.DebugLevel;

            return RealtimeClient.Connect(appSettings.Server, appSettings.Port.ToString(), appSettings.AppId,
                ServerConnection.MVS);
        }

        public static void Disconnect()
        {
            if(RealtimeClient == null) return;
            RemoveAllObjects();
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
        
        // TODO : RPC
        public static bool RPC(ObjectID objectID, string methodName, ulong[] targetPlayers, params object[] args)
        {
            // Debug.Log($"CI : {objectID.ClientInstanceID}, I : {objectID.InstanceID}");
            var fixedData = new C_RPC
            {
                ObjectID = objectID,
                MethodName = HeliosUtility.Compute64BitHash(methodName),
                MethodArgs = ByteString.CopyFrom(HeliosUtility.SerializeParameters(args))
            };
            if (targetPlayers.Length == 0)
            {
                fixedData.Receivers.Add(0);
            }
            else
            {
                foreach (var target in targetPlayers)
                {
                    fixedData.Receivers.Add(target);
                }
            }
            return RaiseEvent((int)Protocol.EventCode.Rpc, fixedData);
        }

        public static async Task<string> GetMvmAddress()
        {
            _heliosSettings.AppSettings.MVM = await RealtimeClient.OpGetMvmAddress();
            IsConnectedToMaster = true;
            return _heliosSettings.AppSettings.MVM;
        }

        public static async Task<List<Room>> GetRoomList()
        {
            await RealtimeClient.OpGetRoomList();
            return RealtimeClient.MvsRoomInfos;
        }

        public static void RoomCreateToMaster(string roomName = default, UInt64 roomId = 0)
        {
            if (CurrentRoom != null)
            {
                RealtimeClient.MVSDebug(DebugLevel.WARNING, "You Already In Room");
                return;
            }
            var res = RealtimeClient.OpCreateAndJoinRoomToMvm(new RoomInfo(){Name = roomName, RoomID = roomId});
            if (res.IP.IsNullOrEmpty()) return;
            HeliosSettings.AppSettings.Server = res.IP;
        }

        public static void RoomJoinToMaster(UInt64 roomId = 0)
        {
            if (CurrentRoom != null)
            {
                RealtimeClient.MVSDebug(DebugLevel.WARNING, "You Already In Room");
                return;
            }
            var res = RealtimeClient.OpJoinRoomToMvm(roomId);
            if (res.IP.IsNullOrEmpty()) return;
            HeliosSettings.AppSettings.Server = res.IP;
        }

        /// <summary>
        /// Direct to MVS
        /// </summary>
        /// <returns></returns>
        public static bool JoinOrCreateRoom(string IPAddress, ulong RoomID, ulong MvsUserID, string MvsUserToken)
        {
            // if (!IsConnectedAndReady) return false;
            RoomJoinInfoStruct opParams = new RoomJoinInfoStruct
            {
                IP = IPAddress,
                RoomID = RoomID,
                MvsUserID = MvsUserID,
                MvsUserToken = MvsUserToken
            };

            return RealtimeClient.OpCreateOrJoinRoomToMvs(opParams);
        }

        public static async Task GetGroupList()
        {
            await RealtimeClient.OpGroupTask();
        }

        public static bool JoinGroup(uint sceneNumber, uint channelID)
        {
            // if (!IsConnectedAndReady) return false;
            // Debug.Log(CurrentGroup);
            if (CurrentRoom == null)
            {
                RealtimeClient.MVSDebug(DebugLevel.WARNING, "You're not in Room");
                return false;
            }
            
            if (CurrentGroup.SceneNumber != sceneNumber ||
                CurrentGroup.ChannelID != channelID)
            {
                RemoveMyObjects();
                
                List<HeliosMonoBehavior> removeObjects = new List<HeliosMonoBehavior>(HeliosObjectList);
                foreach (var obj in removeObjects)
                {
                    if (obj == null)
                    {
                        continue; // obj가 null이면 다음 반복으로 넘어갑니다.
                    }

                    var heliosObject = obj.GetComponentInSelfOrParent<HeliosObject>();
                    if (heliosObject != null)
                    {
                        if (!heliosObject.IsMine)
                        {
                            GameObject.Destroy(obj.gameObject);
                        }
                    }
                }
            }

            return RealtimeClient.OpJoinGroup(sceneNumber, channelID);
        }

        [Obsolete]
        public static bool InitiateObjects()
        {
            if (CurrentGroup == null)
                return false;
            
            return RealtimeClient.OpInitialObjects();
        }

        public static bool RaiseEvent(int eventCode, IMessage fixedData = null, List<Protocol.HeliosVariable> customData = null)
        {
            // if (!InGroup) return false;

            return RealtimeClient.OpRaiseEvent(eventCode, fixedData, customData);
        }

        #region Instantiate

        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (CurrentRoom == null)
                return null;
            var comp = prefab.GetComponentInSelfOrParent<HeliosObject>();
            if (comp != null)
                return Instantiate(comp.PrefabId, position, rotation);
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
            var propPos = objectInfo.Values.Last(x => x.Key == CustomVariables.GetKeyByName("position")).NVector;
            var propRot = objectInfo.Values.Last(x => x.Key == CustomVariables.GetKeyByName("rotation")).NVector;
            Vector3 position = new Vector3((float)propPos.X, (float)propPos.Y, (float)propPos.Z);
            Vector3 rotationV3 = new Vector3((float)propRot.X, (float)propRot.Y, (float)propRot.Z);
            Quaternion rotation = Quaternion.Euler(rotationV3);
            InstantiateParams instantiateParams = new InstantiateParams
            {
                prefabId = objectInfo.ObjectID.PrefabID,
                instanceId = objectInfo.ObjectID.InstanceID,
                clientInstanceID = objectInfo.ObjectID.ClientInstanceID,
                position = position,
                rotation = rotation,
                creator = CurrentRoom.GetPlayer(objectInfo.OwnerPlayerID),
                ObjectInfo = objectInfo
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

            bool isLocalInstantiate = !instantiateEvent && LocalPlayer.Equals(instantiateParams.creator);
            
            go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.Values.Add(new Protocol.HeliosVariable
            {
                Key = CustomVariables.GetKeyByName("position"),
                NVector = new Protocol.Vector3
                {
                    X = instantiateParams.position.x,
                    Y = instantiateParams.position.y,
                    Z = instantiateParams.position.z
                }
            });
            go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.Values.Add(new Protocol.HeliosVariable
            {
                Key = CustomVariables.GetKeyByName("rotation"),
                NVector = new Protocol.Vector3
                {
                    X = instantiateParams.rotation.x,
                    Y = instantiateParams.rotation.y,
                    Z = instantiateParams.rotation.z
                }
            });
            go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.Values.Add(new Protocol.HeliosVariable
            {
                Key = CustomVariables.GetKeyByName("scale"),
                NVector = new Protocol.Vector3
                {
                    X = 1,
                    Y = 1,
                    Z = 1
                }
            });
            
            // TODO : IF Local Instantiate
            if (isLocalInstantiate)
            {
                go.GetComponentInSelfOrParent<HeliosObject>().IsMine = true;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.SyncType = ObjectSyncType.PersonalOwn;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.OwnerPlayerID = instantiateParams.creator.UserId;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.ObjectID.PrefabID = instantiateParams.prefabId;
                HeliosObjectList.Add(go.GetComponentInSelfOrParent<HeliosObject>());
                go.GetComponentInSelfOrParent<HeliosObject>().ClientInstanceId = (uint)HeliosObjectList.LastIndexOf(go.GetComponentInSelfOrParent<HeliosObject>());
                instantiateParams.clientInstanceID = go.GetComponentInSelfOrParent<HeliosObject>().ClientInstanceId;
                // foreach (var heliosMonoBehavior in go.GetComponentsInChildren<HeliosMonoBehavior>())
                // {
                //     // heliosMonoBehavior.FindNetworkedVariables();
                //     // heliosMonoBehavior.FindRPCMethods();
                // }
                instantiateParams.ObjectInfo = go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo; 
                SendInstantiate(instantiateParams, isRoomObject);
            }
            else
            {
                go.GetComponentInSelfOrParent<HeliosObject>().IsMine = false;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo = instantiateParams.ObjectInfo;
                go.GetComponentInSelfOrParent<HeliosObject>().InstanceId = instantiateParams.instanceId;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.SyncType = ObjectSyncType.PersonalOwn;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.OwnerPlayerID = instantiateParams.creator.UserId;
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.ObjectID.PrefabID = instantiateParams.prefabId;
                var saveObjInfo = new ObjectInfo(instantiateParams.ObjectInfo);
                go.GetComponentInSelfOrParent<HeliosObject>().ObjectInfo.Values.Clear();
                go.GetComponentInSelfOrParent<HeliosObject>().ClientInstanceId = instantiateParams.clientInstanceID;
                // foreach (var heliosMonoBehavior in go.GetComponentsInChildren<HeliosMonoBehavior>())
                // {
                //     // heliosMonoBehavior.FindNetworkedVariables();
                //     // heliosMonoBehavior.FindRPCMethods();
                // }
                HeliosObjectList.Add(go.GetComponentInSelfOrParent<HeliosObject>());
                FindObjectById(instantiateParams.instanceId).UpdateCustomData(saveObjInfo);
                RealtimeClient.OnObjectInstantiated(saveObjInfo);
                go.SetActive(_prefabPool.GetPrefabPoolActive(instantiateParams.ObjectInfo.ObjectID.PrefabID));
            }
            
            
            return go;
        }

        internal static bool SendInstantiate(InstantiateParams instantiateParams, bool isRoomObject = false)
        {
            var pkt = new C_ADD_NETWORK_OBJECTS();
            pkt.ObjectInfos.Add(instantiateParams.ObjectInfo);
            return SendEventInternal(EventCode.PKT_C_ADD_NETWORK_OBJECTS, pkt);
        }

        #endregion

        private static void NetworkUpdateObject(uint id, ObjectInfo objectInfo)
        {
            if (objectInfo.SyncType == ObjectSyncType.GroupOwn)
            {
                foreach (var ho in HeliosObjectList)
                {
                    if (ho.ObjectInfo.ObjectID.ClientInstanceID == objectInfo.ObjectID.ClientInstanceID)
                    {
                        
                    }
                }
            }
            else
            {
                var obj = FindObjectById(id);
                if(obj == null) return;
                var propPos = objectInfo.Values.LastOrDefault(x => x.Key == CustomVariables.GetKeyByName("position"))?.NVector;
                var propRot = objectInfo.Values.LastOrDefault(x => x.Key == CustomVariables.GetKeyByName("rotation"))?.NVector;
                var propScale = objectInfo.Values.LastOrDefault(x => x.Key == CustomVariables.GetKeyByName("scale"))?.NVector;
                if (propPos != null)
                {
                    Vector3 position = new Vector3((float)propPos.X, (float)propPos.Y, (float)propPos.Z);
                    obj.GetComponent<HeliosTransform>().networkPosition = position;
                }

                if (propRot != null)
                {
                    Vector3 rotationV3 = new Vector3((float)propRot.X, (float)propRot.Y, (float)propRot.Z);
                    Quaternion rotation = Quaternion.Euler(rotationV3);
                    obj.GetComponent<HeliosTransform>().networkRotation = rotation;
                }

                if (propScale != null)
                {
                    Vector3 scale = new Vector3((float)propScale.X, (float)propScale.Y, (float)propScale.Z);
                    obj.GetComponent<HeliosTransform>().networkScale = scale;
                }
            }
            // FindObjectById(id).ObjectInfo = objectInfo;
            FindObjectById(id).UpdateCustomData(objectInfo);
        }

        internal static void NetworkRemoveObject(uint id)
        {
            _prefabPool.Destroy(id);
        }
        
        private static bool SendEventInternal(int eventCode, IMessage data, List<Protocol.HeliosVariable> customData = null)
        {
            // if (!InRoom)
            // {
            //     Debug.LogWarning("RaiseEvent(" + eventCode + ") failed. You are not in a Room");
            //     return false;
            // }

            return RealtimeClient.OpRaiseEvent(eventCode, data, customData);
        }

        public static long GetCurrentRTT()
        {
            return RealtimeClient.CurRTT;
        }
        

        public static void LoadOrCreateSettings()
        {
            HeliosSettings = (HeliosSettings)Resources.Load(HeliosSettingsFileName, typeof(HeliosSettings));
        }
        
    }
}
