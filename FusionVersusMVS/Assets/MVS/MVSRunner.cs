using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using Protocol;
using UnityEngine;
using Object = UnityEngine.Object;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;

namespace MVS
{
    [AddComponentMenu("MVS/MVS Runner")]
    [DisallowMultipleComponent]
    public class MVSRunner : Singleton<MVSRunner>, MVSNetworkCallbacks, MVSCallbacks
    {
        public static bool isConnected;
        public string authToken;
        public ulong appID;
        public ulong waplRoomID;
        public string name;

        public uint sceneNumber;
        public uint channelID;

        public static bool isRoomJoined = false;
        
        private float _pingTime;

        // StartUp
        internal TaskCompletionSource<bool> _initializeOperation;

        private Action<MVSRunner> _onGameStartAction;

        internal bool OnGameStartedInvoked;

        [NonSerialized] private List<MVSRunnerCallbacks> _callbacks;

        [NonSerialized] internal MVSLoop _mvsLoop;
        
        [NonSerialized]
        private static MVSNetworkConfig _config;
        
        
        /// <summary>
        /// PUBLIC
        /// </summary>

        public MVSNetworkConfig Config => _config;

        // public MVSNetworkObjectTable PrefabTable = _config.NetworkObjectTable; 
        
        // 서버 접속
        private WebSocketHandler _wsh;

        private void Awake()
        {
            if (_callbacks == null)
                _callbacks = new List<MVSRunnerCallbacks>();
            RegisterNetworkCallbacks();
        }

        #region Callbacks

        private void RegisterNetworkCallbacks()
        {
            if (!(bool) (Object) this || !(bool) (Object) gameObject || _callbacks == null || _callbacks.Count != 0)
                return;
            foreach (MVSRunnerCallbacks componentsInChild in gameObject.GetComponentsInChildren<MVSRunnerCallbacks>())
            {
                MonoBehaviour monoBehaviour = componentsInChild as MonoBehaviour;
                if ((bool)(Object)monoBehaviour && monoBehaviour.enabled)
                    AddCallbacks(componentsInChild);
            }
        }

        public void AddCallbacks(params MVSRunnerCallbacks[] callbacks)
        {
            if (_callbacks == null)
                _callbacks = new List<MVSRunnerCallbacks>();
            foreach (MVSRunnerCallbacks callback in callbacks)
            {
                if(!_callbacks.Contains(callback))
                    _callbacks.Add(callback);
            }
        }

        public void RemoveCallbacks(params MVSRunnerCallbacks[] callbacks)
        {
            if (_callbacks == null)
                _callbacks = new List<MVSRunnerCallbacks>();
            foreach (MVSRunnerCallbacks callback in callbacks)
            {
                if(!_callbacks.Contains(callback))
                    _callbacks.Remove(callback);
            }
        }

        internal void InvokeOnGameStartedCallback()
        {
            try
            {
                Action<MVSRunner> onGameStartAction = _onGameStartAction;
                if (onGameStartAction == null)
                    return;
                onGameStartAction(this);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #endregion

        #region Init

        internal Task<bool> Initialize(MVSRunnerInitializeArgs args)
        {
            _initializeOperation = new TaskCompletionSource<bool>();
            _onGameStartAction = args.OnGameStarted;
            OnGameStartedInvoked = false;

            // 각종 설정 여기서 할 것
            
            DontDestroyOnLoad(gameObject);
            
            
            return _initializeOperation.Task;
        }

        #endregion

        #region Object

        public void Spawn(
            GameObject prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            PlayerInfo inputAuthority = null)
        {
            if (prefab == null)
                throw new ArgumentException(nameof(prefab));
            if (!prefab.TryGetComponent<MVSNetworkObject>(out _))
                throw new ArgumentException("No MVSNetworkObject Component", nameof(prefab));
            if (rotation != null && position != null)
                Instantiate(prefab, (Vector3)position, (Quaternion)rotation);

            var pkt = new C_ADD_NETWORK_OBJECTS();
            pkt.ObjectInfos.Add(new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = 1,
                    InstanceID = 0
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = 0
            });
            _wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_ADD_NETWORK_OBJECTS, pkt.ToByteArray(), pkt.CalculateSize());
        }

        // public MVSNetworkObject Spawn(
        //     MVSNetworkObject prefab,
        //     Vector3? position = null,
        //     Quaternion? rotation = null,
        //     PlayerInfo inputAuthority = null)
        // {
        //     if (prefab == null)
        //         throw new ArgumentException(nameof(prefab));
        //     if (!prefab.TryGetComponent<MVSNetworkObject>(out _))
        //         throw new ArgumentException("No MVSNetworkObject Component", nameof(prefab));
        //     if (rotation != null && position != null)
        //         Instantiate(prefab, (Vector3)position, (Quaternion)rotation);
        //     
        //     
        // }
        
        
        #endregion

        public async Task ConnectToMVS()
        {
            // _wsh = new WebSocketHandler();
            _wsh.Init();
            var task = Task.Run(() => _wsh.ConnectServer(this));
            await task;
        }

        public async Task GameStart()
        {
            // await ConnectToMVS();
            // Room생성
            authToken = "Dummy";
            appID = 1;
            waplRoomID = 1;
            name = "UnitTest";
            var pkt = new C_ROOM_JOIN_OR_CREATE
            {
                AuthToken = authToken,
                AppID = appID,
                WaplRoomID = waplRoomID,
                Name = name
            };
            var data = pkt.ToByteArray();
            var size = pkt.CalculateSize();
            _wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_ROOM_JOIN_OR_CREATE, data, size);
            
            sceneNumber = 1;
            channelID = 1;
            
            var pktGroup = new C_GROUP_JOIN
            {
                GroupID = new GroupID
                {
                    SceneNumber = sceneNumber,
                    ChannelID = channelID
                }
            };
            var dataG = pktGroup.ToByteArray();
            var sizeG = pktGroup.CalculateSize();
            _wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_GROUP_JOIN, dataG, sizeG);

            var pktInitObject = new C_INITIAL_OBJECTS();
            _wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_INITIAL_OBJECTS, pktInitObject.ToByteArray(), pktInitObject.CalculateSize());
        }

        public void SendChat()
        {
            var pkt = new C_CHAT
            {
                Msg = "TestChat"
            };
            var data = pkt.ToByteArray();
            var size = pkt.CalculateSize();
            _wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_CHAT, data, size);
        }

        private void Update()
        {
            if (!isConnected) return;
            _wsh.ProcessReceiveData();

            _pingTime -= Time.deltaTime;
            if (_pingTime < -0.2f)
            {
                C_HEART_BEAT packet = new C_HEART_BEAT();
                _wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_HEART_BEAT, packet.ToByteArray(), packet.CalculateSize());
                _pingTime = 0;
            }
        }

        public void OnConnected()
        {
            for (int i = 0; i < _callbacks.Count; ++i)
            {
                _callbacks[i].OnConnectedToServer(this);
            }
        }

        public void OnConnectedToMaster()
        {
            
        }

        public void OnDisconnected()
        {
            for (int i = 0; i < _callbacks.Count; ++i)
            {
                _callbacks[i].OnShutDown(this);
            }
        }

        public void OnRoomJoined()
        {
            for (int i = 0; i < _callbacks.Count; ++i)
            {
                _callbacks[i].OnRoomJoined(this);
            }
        }

        public void OnGroupJoined()
        {
            
        }

        public void OnPlayerJoined(PlayerInfo playerInfo)
        {
            for (int i = 0; i < _callbacks.Count; ++i)
            {
                _callbacks[i].OnPlayerJoined(this, playerInfo);
            }
        }

        public void OnPlayerLeft(PlayerInfo playerInfo)
        {
            for (int i = 0; i < _callbacks.Count; ++i)
            {
                _callbacks[i].OnPlayerLeft(this, playerInfo);
            }
        }

        public void OnInput()
        {
            
        }
    }
}
