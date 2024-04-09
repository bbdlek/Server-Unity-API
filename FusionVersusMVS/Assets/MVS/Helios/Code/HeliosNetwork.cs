using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using MVS.Realtime;
using Protocol;
using UnityEditor;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = MVS.Realtime.HeliosVariable;
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
        
        public static float SendRate
        {
            get
            {
                return 1000f / _sendFrequency;
            }

            set
            {
                _sendFrequency = 1000f / value;
            }
        }

        private static float _sendFrequency = 33f; // in milliseconds.

        public static List<HeliosVariable> HeliosVariables = new List<HeliosVariable>();
        // public static Dictionary<int, HeliosVariable> HeliosVariableDic = new Dictionary<int, HeliosVariable>();
        
        public static bool IsMessageQueueRunning
        {
            get
            {
                return _isMessageQueueRunning;
            }

            set
            {
                _isMessageQueueRunning = value;
            }
        }
        
        private static bool _isMessageQueueRunning = true;
        
        public static float MinimalTimeScaleToDispatchInFixedUpdate = -1f;

        public static List<Room> RoomList => RealtimeClient == null ? null : RealtimeClient.RoomList;

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
            RealtimeClient = new RealtimeClient();
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
            CustomVariablesUnity.Register();

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

            RealtimeClient.AppSettingsDebug = appSettings.DebugLevel;

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
        
        // TODO : RPC
        public static bool RPC(uint instanceID, string methodName)
        {
            var fixedData = new C_RPC
            {
                InstanceID = instanceID,
                MethodName = methodName
            };
            return RaiseEvent((int)Protocol.EventCode.Rpc, fixedData);
        }

        public static async Task GetRoomList()
        {
            await RealtimeClient.OpRoomTask();
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

        public static async Task GetGroupList()
        {
            await RealtimeClient.OpGroupTask();
        }

        public static bool JoinGroup(uint sceneNumber, uint channelID)
        {
            // if (!IsConnectedAndReady) return false;

            return RealtimeClient.OpJoinGroup(sceneNumber, channelID);
        }

        public static bool RaiseEvent(int eventCode, IMessage fixedData = null, CustomDic customData = null)
        {
            // if (!InGroup) return false;

            return RealtimeClient.OpRaiseEvent(eventCode, fixedData, customData);
        }

        #region Instantiate

        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (CurrentRoom == null)
                return null;
            var comp = prefab.GetComponent<HeliosObject>();
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
            // var propPos = objectInfo.NumberProps.FirstOrDefault(prop => prop.Index == PropsID.Position3D).Value;
            // var propRot = objectInfo.NumberProps.FirstOrDefault(prop => prop.Index == PropsID.Rotation3D).Value;
            var propPos = objectInfo.CustomValues.Params["Position"].NVector;
            var propRot = objectInfo.CustomValues.Params["Rotation"].NVector;
            Vector3 position = new Vector3((float)propPos.X, (float)propPos.Y, (float)propPos.Z);
            Vector3 rotationV3 = new Vector3((float)propRot.X, (float)propRot.Y, (float)propRot.Z);
            Quaternion rotation = Quaternion.Euler(rotationV3);
            // Quaternion rotation = new Quaternion((float)propRot[0], (float)propRot[1], (float)propRot[2], (float)propRot[3]);
            InstantiateParams instantiateParams = new InstantiateParams
            {
                prefabId = objectInfo.ObjectID.PrefabID,
                instanceId = objectInfo.ObjectID.InstanceID,
                clientInstanceID = objectInfo.ObjectID.ClientInstanceID,
                position = position,
                rotation = rotation,
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
            
            // TODO : IF Local Instantiate
            if (isLocalInstantiate)
            {
                foreach (var ho in go.GetComponentsInChildren<HeliosObject>())
                {
                    ho.IsMine = true;
                    ho.FindNetworkedVariables();
                    ho.FindHeliosVariable();
                    HeliosObjectList.Add(ho);
                    ho.ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(ho);
                    ho.ObjectInfo.SyncType = ObjectSyncType.PersonalOwn;
                    instantiateParams.clientInstanceID = ho.ObjectInfo.ObjectID.ClientInstanceID;
                }
                SendInstantiate(instantiateParams, isRoomObject);
            }
            else
            {
                go.GetComponent<HeliosObject>().InstanceId = instantiateParams.instanceId;
                foreach (var ho in go.GetComponentsInChildren<HeliosObject>())
                {
                    ho.InstanceId = instantiateParams.instanceId;
                    ho.IsMine = false;
                    ho.FindNetworkedVariables();
                    ho.FindHeliosVariable();
                    HeliosObjectList.Add(ho);
                    ho.ObjectInfo.ObjectID.ClientInstanceID = (uint)HeliosNetwork.HeliosObjectList.LastIndexOf(ho);
                    ho.ObjectInfo.SyncType = ObjectSyncType.PersonalOwn;
                    // heliosMonoBehavior.SetHeliosVariableIndex();
                }
                HeliosObjectList.Add(go.GetComponent<HeliosObject>());
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
                    ClientInstanceID = instantiateParams.clientInstanceID
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = LocalPlayer.UserId,
            };
            var paramDic = new CustomDic();
            paramDic.Params.Add("Position", new Protocol.HeliosVariable()
            {
                NVector = new Protocol.Vector3
                {
                    X = instantiateParams.position.x,
                    Y = instantiateParams.position.y,
                    Z = instantiateParams.position.z
                }
            });
            Quaternion rot = new Quaternion
            {
                x = instantiateParams.rotation.x,
                y = instantiateParams.rotation.y,
                z = instantiateParams.rotation.z,
                w = instantiateParams.rotation.w,
            };
            Vector3 rotation = rot.eulerAngles;
            paramDic.Params.Add("Rotation", new Protocol.HeliosVariable
            {
                NVector = new Protocol.Vector3
                {
                    X = rotation.x,
                    Y = rotation.y,
                    Z = rotation.z
                }
            });
            ObjectInfo.CustomValues = paramDic;
            pkt.ObjectInfos.Add(ObjectInfo);
            return SendEventInternal(EventCode.PKT_C_ADD_NETWORK_OBJECTS, pkt);
        }

        #endregion

        private static void NetworkUpdateObject(uint id, ObjectInfo objectInfo)
        {
            if (objectInfo.SyncType == ObjectSyncType.GlobalOwn)
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
                // Version 1
                var propPos = objectInfo.CustomValues.Params["Position"].NVector;
                var propRot = objectInfo.CustomValues.Params["Rotation"].NVector;
                Vector3 position = new Vector3((float)propPos.X, (float)propPos.Y, (float)propPos.Z);
                Vector3 rotationV3 = new Vector3((float)propRot.X, (float)propRot.Y, (float)propRot.Z);
                Quaternion rotation = Quaternion.Euler(rotationV3);
            
                // Version 2
                var propPos2 = objectInfo.TestValues.FirstOrDefault(variable => variable.Key == CustomVariables.GetKeyByName("position"))?.NVector;
                var propRot2 = objectInfo.TestValues.FirstOrDefault(variable => variable.Key == CustomVariables.GetKeyByName("rotation"))?.NVector;
                Vector3 position2 = new Vector3((float)propPos2.X, (float)propPos2.Y, (float)propPos2.Z);
                Vector3 rotation2V3 = new Vector3((float)propRot2.X, (float)propRot2.Y, (float)propRot2.Z);
                Quaternion rotation2 = Quaternion.Euler(rotation2V3);

                var obj = FindObjectById(id);
                obj.GetComponent<HeliosTransform>().networkPosition = position2;
                obj.GetComponent<HeliosTransform>().networkRotation = rotation2;
            }
            
            FindObjectById(id).ObjectInfo = objectInfo;
            FindObjectById(id).UpdateCustomData();
        }

        private static void NetworkRemoveObject(uint id)
        {
            if (HeliosObjectList.Find(x => x.ObjectInfo.ObjectID.InstanceID == id))
            {
                var obj = HeliosObjectList.Find(x => x.ObjectInfo.ObjectID.InstanceID == id);
                // foreach (var heliosMonoBehavior in obj.GetComponentsInChildren<HeliosMonoBehavior>())
                // {
                //     foreach (var heliosVariable in heliosMonoBehavior.HeliosVariableTable)
                //     {
                //         HeliosVariableDic.Remove(heliosVariable.Index);
                //     }
                // }
                GameObject.Destroy(obj.gameObject);
                HeliosObjectList.Remove(obj);
            }
        }
        
        private static bool SendEventInternal(int eventCode, IMessage data, CustomDic customData = null)
        {
            // if (!InRoom)
            // {
            //     Debug.LogWarning("RaiseEvent(" + eventCode + ") failed. You are not in a Room");
            //     return false;
            // }

            return RealtimeClient.OpRaiseEvent(eventCode, data, customData);
        }
        

        public static void LoadOrCreateSettings()
        {
            HeliosSettings = (HeliosSettings)Resources.Load(HeliosSettingsFileName, typeof(HeliosSettings));
        }
        
    }
}
