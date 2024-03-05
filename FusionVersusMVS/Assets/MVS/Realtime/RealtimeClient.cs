using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Protocol;
using UnityEngine;

namespace MVS.Realtime
{
    #region Enum

    public enum ClientState
    {
        Created,
        Authenticating,
        JoiningRoom,
        JoinedRoom,
        LeavingRoom,
        JoiningGroup,
        JoinedGroup,
        LeavingGroup,
        
        ConnectingToNameServer,
        ConnectedToNameServer,
        DisConnectingFromNameServer,
        
        ConnectingToMasterServer,
        ConnectedToMasterServer,
        DisConnectingFromMasterServer,
        
        ConnectingToMVS,
        ConnectedToMVS,
        DisConnectingFromMVS,
        
        Disconnecting,
        DisConnected
        
    }

    public enum DisconnectedReason
    {
        None,
        MaxCCU,
        ApplicationQuit,
        DisconnectByClient,
        DisconnectByDisconnectReason
    }

    public enum ServerConnection
    {
        MasterServer,
        NameServer,
        MVS
    }

    // public struct MVSPortDefinition
    // {
    //     public static readonly MVSPortDefinition PortDefinition = new MVSPortDefinition()
    //         { NameServerPort = "9999", MasterServerPort = "1231", MVSPort = "30080" };
    //
    //     public string NameServerPort;
    //     public string MasterServerPort;
    //     public string MVSPort;
    // }

    #endregion

    public class RealtimeClient : IRealtimePeerListener
    {
        public RealtimePeer RealtimePeer { get; private set; }

        public string AppVersion { get; set; }

        public string AppId { get; set; }

        // public AuthenticationValues AuthValues { get; set; }

        public AuthModeOption AuthMode = AuthModeOption.None;

        public ConnectionProtocol? ConnectionProtocol { get; set; }

        // public object TokenForInit
        // {
        //     get
        //     {
        //         if (AuthMode == AuthModeOption.Auth)
        //         {
        //             return null;
        //         }
        //
        //         return AuthValues?.Token;
        //     }
        // }

        private object tokenCache;
        
        public bool IsUsingNameServer { get; set; }
        
        public string NameServerAddress { get; set; }
        
        public string MasterServerAddress { get; set; }
        
        public string MVSAddress { get; set; }
        
        public bool IsDirectToMVS { get; set; }
        
        public ServerConnection Server { get; private set; }

        private ClientState state = ClientState.Created;

        public ClientState State
        {
            get => state;
            set
            {
                if (state == value)
                {
                    return;
                }
                ClientState previousState = state;
                state = value;
                StateChanged?.Invoke(previousState, state);
            }
        }

        public bool IsConnected => RealtimePeer != null && State != ClientState.Created && State != ClientState.DisConnected;

        public bool IsConnectedAndReady
        {
            get
            {
                if (RealtimePeer == null)
                {
                    return false;
                }

                switch (State)
                {
                    case ClientState.Created:
                    case ClientState.DisConnected:
                    case ClientState.DisConnectingFromMVS:
                    case ClientState.DisConnectingFromMasterServer:
                    case ClientState.DisConnectingFromNameServer:
                    case ClientState.Authenticating:
                    case ClientState.ConnectingToMVS:
                    case ClientState.ConnectingToMasterServer:
                    case ClientState.ConnectingToNameServer:
                    case ClientState.JoiningRoom:
                    case ClientState.JoiningGroup:
                    case ClientState.LeavingRoom:
                    case ClientState.LeavingGroup:
                        return false;
                }

                return true;
            }
        }
        
        public event Action<ClientState, ClientState> StateChanged;
        
        public event Action<EventData> EventReceived;
        
        public event Action<OperationResponse> OpResponseReceived;

        public ConnectionCallbacksContainer ConnectionCallbacksTarget;
        public MakingRoomCallbacksContainer MakingRoomCallbacksTarget;
        public MakingGroupCallbacksContainer MakingGroupCallbacksTarget;
        internal InRoomCallbacksContainer InRoomCallbacksTarget;
        internal InGroupCallbacksContainer InGroupCallbacksTarget;
        internal ErrorInfoCallbacksContainer ErrorInfoCallbacksTarget;
        
        public DisconnectedReason DisconnectedReason { get; protected set; }

        public bool InRoom => State == ClientState.JoinedRoom;
        
        public bool InGroup => State == ClientState.JoinedGroup;
        
        public Player LocalPlayer { get; internal set; }
        
        public Room CurrentRoom { get; internal set; }
        
        public Group CurrentGroup { get; internal set; }
        
        private class CallbackTargetChange
        {
            public readonly object Target;
            public readonly bool AddTarget;

            public CallbackTargetChange(object target, bool addTarget)
            {
                this.Target = target;
                this.AddTarget = addTarget;
            }
        }

        private readonly Queue<CallbackTargetChange> _callbackTargetChanges = new Queue<CallbackTargetChange>();
        private readonly HashSet<object> _callbackTargets = new HashSet<object>();

        public RealtimeClient(ConnectionProtocol protocol = Realtime.ConnectionProtocol.Tcp)
        {
            ConnectionCallbacksTarget = new ConnectionCallbacksContainer(this);
            MakingRoomCallbacksTarget = new MakingRoomCallbacksContainer(this);
            MakingGroupCallbacksTarget = new MakingGroupCallbacksContainer(this);
            InRoomCallbacksTarget = new InRoomCallbacksContainer(this);
            InGroupCallbacksTarget = new InGroupCallbacksContainer(this);
            ErrorInfoCallbacksTarget = new ErrorInfoCallbacksContainer(this);

            RealtimePeer = new RealtimePeer(this, protocol);
            RealtimePeer.OnDisconnectReason += OnDisconnectMessageReceived;
            LocalPlayer = CreatePlayer(true, new PlayerInfo
            {
                PlayerID = 0,
                Name = "Player"
            });
            
            #if SUPPORTED_UNITY
            //CustomType Register
            #endif
            
            State = ClientState.Created;
            MVSDebug(DebugLevel.INFO, "PeerCreate");
        }

        #region NameServer

        public int NameServerPortInAppSettings;

        #endregion

        #region Operations and Commands

        public virtual bool ConnectUsingSettings(AppSettings appSettings)
        {
            if (RealtimePeer.PeerState != PeerState.Disconnected)
            {
                MVSDebug(DebugLevel.WARNING, "ConnectUsingSettings() failed, PeerState is not Disconnected");
                return false;
            }

            if (appSettings == null)
            {
                MVSDebug(DebugLevel.ERROR, "ConnectUsingSettings() failed. AppSettings is null");
                return false;
            }

            AppId = appSettings.AppId;
            AppVersion = appSettings.AppVersion;
            IsUsingNameServer = appSettings.IsUseNameServer;
            ConnectionProtocol = appSettings.Protocol;
            DisconnectedReason = DisconnectedReason.None;

            if (IsUsingNameServer)
            {
                Server = ServerConnection.NameServer;
                if (!RealtimePeer.Connect(NameServerAddress + ":" + appSettings.Port, AppId, ServerConnection.NameServer))
                {
                    return false;
                }
                State = ClientState.ConnectingToNameServer;
            }
            else if(IsDirectToMVS)
            {
                Server = ServerConnection.MVS;
                if (!RealtimePeer.Connect(MVSAddress + ":" + appSettings.Port, AppId, ServerConnection.MVS))
                {
                    return false;
                }
                State = ClientState.ConnectingToMVS;
            }
            else
            {
                Server = ServerConnection.MasterServer;
                if (!RealtimePeer.Connect(MasterServerAddress + ":" + appSettings.Port, AppId, ServerConnection.MasterServer))
                {
                    return false;
                }
                State = ClientState.ConnectingToMasterServer;
            }

            return true;
        }

        #endregion
        
        public bool Connect(string serverAddress, string serverPort, string appId, ServerConnection serverType)
        {
            if (State == ClientState.Disconnecting)
            {
                MVSDebug(DebugLevel.ERROR, $"Connect() failed because Current State is {State}");
                return false;
            }

            DisconnectedReason = DisconnectedReason.None;

            bool connecting = RealtimePeer.Connect(serverAddress + ":" + serverPort, appId, serverType);
            if (connecting)
            {
                Server = serverType;
                switch (serverType)
                {
                    case ServerConnection.NameServer:
                        State = ClientState.ConnectingToNameServer;
                        break;
                    case ServerConnection.MasterServer:
                        State = ClientState.ConnectingToMasterServer;
                        break;
                    case ServerConnection.MVS:
                        State = ClientState.ConnectingToMVS;
                        break;

                }
            }

            return connecting;
        }

        public void Disconnect(DisconnectedReason disconnectedReason = DisconnectedReason.DisconnectByClient)
        {
            if (State != ClientState.DisConnected)
            {
                State = ClientState.Disconnecting;
                DisconnectedReason = disconnectedReason;
                RealtimePeer.Disconnect();
            }
        }

        public void DisconnectToReconnect()
        {
            switch (Server)
            {
                case ServerConnection.NameServer:
                    State = ClientState.DisConnectingFromNameServer;
                    break;
                case ServerConnection.MasterServer:
                    State = ClientState.DisConnectingFromMasterServer;
                    break;
                case ServerConnection.MVS:
                    State = ClientState.DisConnectingFromMVS;
                    break;
            }

            RealtimePeer.Disconnect();
        }

        public void Service()
        {
            if (RealtimePeer != null)
            {
                RealtimePeer.Service();
            }
        }
        
        public virtual bool SendEvent(EventCode eventCode, byte[] data, int size)
        {
            return this.RealtimePeer.SendEvent(eventCode, data, size);
        }

        public virtual void MVSDebug(DebugLevel debugLevel, string msg)
        {
            if (debugLevel == DebugLevel.ERROR)
            {
                Debug.LogError(msg);
            }
            else if (debugLevel == DebugLevel.WARNING)
            {
                Debug.LogWarning(msg);
            }
            else if (debugLevel == DebugLevel.INFO)
            {
                Debug.Log(msg);
            }
            else if (debugLevel == DebugLevel.ALL)
            {
                Debug.Log(msg);
            }
        }

        public virtual void OnStatusChanged(StatusCode statusCode)
        {
            MVSDebug(DebugLevel.INFO, $"StatusCode : {statusCode.ToString()}");
            MVSDebug(DebugLevel.INFO, $"ClientState : {State.ToString()}");
            switch (statusCode)
            {
                case StatusCode.Connect:
                    switch (State)
                    {
                        case ClientState.ConnectingToNameServer:
                            State = ClientState.ConnectedToNameServer;
                            MVSDebug(DebugLevel.INFO, "ConnectingToNameServer");
                            break;
                        case ClientState.ConnectingToMasterServer:
                            State = ClientState.ConnectedToMasterServer;
                            MVSDebug(DebugLevel.INFO, "ConnectingToMasterServer");
                            break;
                        case ClientState.ConnectingToMVS:
                            MVSDebug(DebugLevel.INFO, "ConnectingToMVS");
                            State = ClientState.ConnectedToMVS;
                            ConnectionCallbacksTarget.OnConnected();
                            break;
                    }
                    break;
                case StatusCode.Disconnect:
                    switch (State)
                    {
                        case ClientState.Created:
                            break;
                        case ClientState.DisConnectingFromMasterServer:
                            MVSDebug(DebugLevel.INFO, "DisconnectedFromMasterServer");
                            break;
                        case ClientState.DisConnectingFromNameServer:
                            MVSDebug(DebugLevel.INFO, "DisconnectedFromNameServer");
                            break;
                        case ClientState.DisConnectingFromMVS:
                            MVSDebug(DebugLevel.INFO, "DisconnectedFromMVS");
                            break;
                    }
                    ConnectionCallbacksTarget.OnDisconnected();
                    break;
                case StatusCode.Exception:
                    break;
                case StatusCode.SendError:
                    break;  
            }
        }

        public virtual void OnEvent(EventData eventData)
        {
            // Player player = CurrentRoom != null ? CurrentRoom.GetPlayer(eventData.Sender) : null;
            EventCode eventCode = eventData.code;
            
            switch (eventCode)
            {
                case EventCode.PKT_S_CHAT:
                    break;
                case EventCode.PKT_S_ROOM_JOIN_OR_CREATE:
                    MakingRoomCallbacksTarget.OnJoinedRoom();
                    break;
                case EventCode.PKT_S_GROUP_JOIN:
                    MakingGroupCallbacksTarget.OnJoinedGroup();
                    var pkt = new C_PLAYER_ID();
                    OpRaiseEvent(EventCode.PKT_C_PLAYER_ID, pkt);
                    break;
                case EventCode.PKT_S_PLAYER_ID:
                    var data_PlayerID = S_PLAYER_ID.Parser.ParseFrom(eventData.Data);
                    LocalPlayer.PlayerInfo.PlayerID = data_PlayerID.PlayerID;
                    Debug.Log(LocalPlayer.UserId);
                    break;
                case EventCode.PKT_S_OTHER_CLIENT_JOINED:
                    var data = S_OTHER_CLIENT_JOINED.Parser.ParseFrom(eventData.Data);
                    Player otherPlayer = new Player(data.PlayerInfo);
                    InGroupCallbacksTarget.OnPlayerEnteredGroup(otherPlayer);
                    break;
            }
            UpdateCallbackTargets();
            if (EventReceived != null)
            {
                EventReceived(eventData);
            }
        }
        
        private void OnDisconnectMessageReceived(DisconnectedReason reason)
        {
            MVSDebug(DebugLevel.ERROR, $"Got DisconnectMessage, Reason : {reason.ToString()}");
            Disconnect(DisconnectedReason.DisconnectByDisconnectReason);
        }

        public virtual void OnOperationResponse(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode == 1)
            {
                //ERRRRRROR
                return;
            }

            switch (operationResponse.OperationCode)
            {
                case OperationCode.Authenticate:
                    if (operationResponse.ReturnCode != 0)
                    {
                        MVSDebug(DebugLevel.ERROR, operationResponse.ToString());
                        switch (operationResponse.ReturnCode)
                        {
                            //ErrorCode
                        }
                        Disconnect(DisconnectedReason);
                    }

                    if (Server == ServerConnection.NameServer)
                    {
                        //ToMaster
                        DisconnectToReconnect();
                    }

                    if (Server == ServerConnection.MasterServer)
                    {
                        //ToMVS
                    }

                    if (Server == ServerConnection.MVS)
                    {
                        
                    }
                    break;
                case OperationCode.JoinRoom:
                    JoinRoom(operationResponse);
                    break;
                case OperationCode.JoinGroup:
                    JoinGroup(operationResponse);
                    break;
            }

            OpResponseReceived?.Invoke(operationResponse);
        }
        
        public void AddCallbackTarget(object target)
        {
            this._callbackTargetChanges.Enqueue(new CallbackTargetChange(target, true));
        }
        
        public void RemoveCallbackTarget(object target)
        {
            this._callbackTargetChanges.Enqueue(new CallbackTargetChange(target, false));
        }
        
        protected internal void UpdateCallbackTargets()
        {
            while (this._callbackTargetChanges.Count > 0)
            {
                CallbackTargetChange change = this._callbackTargetChanges.Dequeue();

                if (change.AddTarget)
                {
                    if (this._callbackTargets.Contains(change.Target))
                    {
                        //Debug.Log("UpdateCallbackTargets skipped adding a target, as the object is already registered. Target: " + change.Target);
                        continue;
                    }

                    this._callbackTargets.Add(change.Target);
                }
                else
                {
                    if (!this._callbackTargets.Contains(change.Target))
                    {
                        //Debug.Log("UpdateCallbackTargets skipped removing a target, as the object is not registered. Target: " + change.Target);
                        continue;
                    }

                    this._callbackTargets.Remove(change.Target);
                }
                
                UpdateCallbackTarget(change, ConnectionCallbacksTarget);
                UpdateCallbackTarget(change, MakingRoomCallbacksTarget);
                UpdateCallbackTarget(change, MakingGroupCallbacksTarget);
                UpdateCallbackTarget(change, InRoomCallbacksTarget);
                UpdateCallbackTarget(change, InGroupCallbacksTarget);
                UpdateCallbackTarget(change, ErrorInfoCallbacksTarget);
                
                IOnEventCallbacks onEventCallback = change.Target as IOnEventCallbacks;
                if (onEventCallback != null)
                {
                    if (change.AddTarget)
                    {
                        EventReceived += onEventCallback.OnEvent;
                    }
                    else
                    {
                        EventReceived -= onEventCallback.OnEvent;
                    }
                }
            }
        }
        
        private void UpdateCallbackTarget<T>(CallbackTargetChange change, List<T> container) where T : class
        {
            T target = change.Target as T;
            if (target != null)
            {
                if (change.AddTarget)
                {
                    container.Add(target);
                }
                else
                {
                    container.Remove(target);
                }
            }
        }

        private void JoinRoom(OperationResponse operationResponse)
        {
            var data = S_ROOM_JOIN_OR_CREATE.Parser.ParseFrom(operationResponse.Data);
            RoomInfo newRoomInfo = new RoomInfo
            {
                AppID = data.AppID,
                RoomID = data.WaplRoomID,
                Name = data.Name
            };
            CurrentRoom = CreateRoom(newRoomInfo);
            CurrentRoom.RealtimeClient = this;
            CurrentRoom.StorePlayer(LocalPlayer);

            State = ClientState.JoinedRoom;
            MakingRoomCallbacksTarget.OnJoinedRoom();
        }

        protected internal virtual Room CreateRoom(RoomInfo roomInfo)
        {
            Room newRoom = new Room(roomInfo);
            return newRoom;
        }

        private void JoinGroup(OperationResponse operationResponse)
        {
            var data = S_GROUP_JOIN.Parser.ParseFrom(operationResponse.Data);
            GroupInfo groupInfo = data.GroupInfo;
            Group newGroup = new Group(groupInfo, CurrentRoom);

            CurrentGroup = newGroup;
            CurrentGroup.RealtimeClient = this;

            State = ClientState.JoinedGroup;
            MakingGroupCallbacksTarget.OnJoinedGroup();
        }

        protected internal virtual Player CreatePlayer(bool isLocal, PlayerInfo playerInfo)
        {
            Player newPlayer = new Player(playerInfo);
            return newPlayer;
        }

        private bool CheckOpCanBeSent(byte opCode, ServerConnection serverConnection, string opName)
        {
            if (RealtimePeer == null)
            {
                MVSDebug(DebugLevel.ERROR, $"Operation {opName} ({opCode}) can't be sent because peer is null");
                return false;
            }

            return true;
        }
        
        //Functions
        public bool OpCreateRoom(string AuthToken, ulong AppID, ulong WaplRoomID, string Name)
        {
            var CreateRoomPkt = new C_ROOM_JOIN_OR_CREATE
            {
                AuthToken = AuthToken,
                AppID = AppID,
                WaplRoomID = WaplRoomID,
                Name = Name
            };
            if (!CheckOpCanBeSent((byte)OperationCode.CreateRoom, Server, "CreateRoom"))
            {
                return false;
            }

            bool sent = RealtimePeer.OpCreateRoom(CreateRoomPkt);

            return sent;
        }

        public bool OpJoinGroup(uint sceneNumber, uint channelID)
        {
            var JoinGroupPkt = new C_GROUP_JOIN
            {
                GroupID = new GroupID
                {
                    SceneNumber = sceneNumber,
                    ChannelID = channelID
                }
            };
            if (!CheckOpCanBeSent((byte)OperationCode.JoinGroup, Server, "JoinGroup"))
            {
                return false;
            }

            bool sent = RealtimePeer.OpJoinGroup(JoinGroupPkt);

            return sent;
        }

        public bool OpAddNetworkObject()
        {
            var AddNetworkObjectPkt = new C_ADD_NETWORK_OBJECTS();
            AddNetworkObjectPkt.ObjectInfos.Add(new ObjectInfo
            {
                ObjectID = new ObjectID
                {
                    PrefabID = 1,
                    InstanceID = 1
                },
                SyncType = ObjectSyncType.PersonalOwn,
                OwnerPlayerID = 1
            });
            
            //CheckOpCanBeSent
            bool sent = RealtimePeer.OpAddNetworkObject(AddNetworkObjectPkt);

            return sent;
        }
        
        public virtual bool OpRaiseEvent(EventCode EventCode, IMessage pkt)
        {
            var data = pkt.ToByteArray();
            var size = pkt.CalculateSize();
            if (!CheckOpCanBeSent((byte)OperationCode.RaiseEvent, Server, "RaiseEvent"))
            {
                return false;
            }

            return RealtimePeer.SendEvent(EventCode, data, size);
        }
    }

    public interface IConnectionCallbacks
    {
        void OnConnected();
        
        void OnConnectedToMaster();

        void OnDisconnected();
        
        void OnCustomAuthenticationResponse(Dictionary<string, object> data);
        
        void OnCustomAuthenticationFailed(string debugMessage);
    }

    public interface IMakingRoomCallbacks
    {
        void OnCreatedRoom();

        void OnCreatedRoomFailed(short failCode, string message);

        void OnJoinedRoom();

        void OnJoinedRoomFailed(short failCode, string message);

        void OnLeftRoom();
    }
    
    public interface IMakingGroupCallbacks
    {
        void OnCreatedGroup();

        void OnCreatedGroupFailed(short failCode, string message);

        void OnJoinedGroup();

        void OnJoinedGroupFailed(short failCode, string message);

        void OnLeftGroup();
    }

    public interface IInRoomCallbacks
    {
        void OnPlayerEnteredRoom(Player newPlayer);
        
        void OnPlayerLeftRoom(Player otherPlayer);
        
        void OnMasterClientSwitched(Player newMasterClient);
    }
    
    public interface IInGroupCallbacks
    {
        void OnPlayerEnteredGroup(Player newPlayer);
        
        void OnPlayerLeftGroup(Player otherPlayer);
    }

    public interface IOnEventCallbacks
    {
        void OnEvent(EventData eventData);
    }

    public interface IErrorInfoCallbacks
    {
        void OnErrorInfo();
    }
    
    public class ConnectionCallbacksContainer : List<IConnectionCallbacks>, IConnectionCallbacks
    {
        private readonly RealtimeClient _client;

        public ConnectionCallbacksContainer(RealtimeClient client)
        {
            _client = client;
        }

        public void OnConnected()
        {
            _client.UpdateCallbackTargets();
            
            foreach (IConnectionCallbacks target in this)
            {
                target.OnConnected();
            }
        }

        public void OnConnectedToMaster()
        {
            _client.UpdateCallbackTargets();
            
            foreach (IConnectionCallbacks target in this)
            {
                target.OnConnectedToMaster();
            }
        }

        public void OnDisconnected()
        {
            _client.UpdateCallbackTargets();
            
            foreach (IConnectionCallbacks target in this)
            {
                target.OnDisconnected();
            }
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            _client.UpdateCallbackTargets();
            
            foreach (IConnectionCallbacks target in this)
            {
                target.OnCustomAuthenticationResponse(data);
            }
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            _client.UpdateCallbackTargets();
            
            foreach (IConnectionCallbacks target in this)
            {
                target.OnCustomAuthenticationFailed(debugMessage);
            }
        }
    }

    public class MakingRoomCallbacksContainer : List<IMakingRoomCallbacks>, IMakingRoomCallbacks
    {
        private readonly RealtimeClient _client;

        public MakingRoomCallbacksContainer(RealtimeClient client)
        {
            _client = client;
        }
        
        public void OnCreatedRoom()
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingRoomCallbacks target in this)
            {
                target.OnCreatedRoom();
            }
        }

        public void OnCreatedRoomFailed(short failCode, string message)
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingRoomCallbacks target in this)
            {
                target.OnCreatedRoomFailed(failCode, message);
            }
        }

        public void OnJoinedRoom()
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingRoomCallbacks target in this)
            {
                target.OnJoinedRoom();
            }
        }

        public void OnJoinedRoomFailed(short failCode, string message)
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingRoomCallbacks target in this)
            {
                target.OnJoinedRoomFailed(failCode, message);
            }
        }

        public void OnLeftRoom()
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingRoomCallbacks target in this)
            {
                target.OnLeftRoom();
            }
        }
    }

    public class MakingGroupCallbacksContainer : List<IMakingGroupCallbacks>, IMakingGroupCallbacks
    {
        private readonly RealtimeClient _client;

        public MakingGroupCallbacksContainer(RealtimeClient client)
        {
            _client = client;
        }
        
        public void OnCreatedGroup()
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingGroupCallbacks target in this)
            {
                target.OnCreatedGroup();
            }
        }

        public void OnCreatedGroupFailed(short failCode, string message)
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingGroupCallbacks target in this)
            {
                target.OnCreatedGroupFailed(failCode, message);
            }
        }

        public void OnJoinedGroup()
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingGroupCallbacks target in this)
            {
                target.OnJoinedGroup();
            }
        }

        public void OnJoinedGroupFailed(short failCode, string message)
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingGroupCallbacks target in this)
            {
                target.OnJoinedGroupFailed(failCode, message);
            }
        }

        public void OnLeftGroup()
        {
            _client.UpdateCallbackTargets();

            foreach (IMakingGroupCallbacks target in this)
            {
                target.OnLeftGroup();
            }
        }
    }

    internal class InRoomCallbacksContainer : List<IInRoomCallbacks>, IInRoomCallbacks
    {
        private readonly RealtimeClient _client;

        public InRoomCallbacksContainer(RealtimeClient client)
        {
            _client = client;
        }
        
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            _client.UpdateCallbackTargets();

            foreach (IInRoomCallbacks target in this)
            {
                target.OnPlayerEnteredRoom(newPlayer);
            }
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            _client.UpdateCallbackTargets();

            foreach (IInRoomCallbacks target in this)
            {
                target.OnPlayerLeftRoom(otherPlayer);
            }
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            _client.UpdateCallbackTargets();

            foreach (IInRoomCallbacks target in this)
            {
                target.OnMasterClientSwitched(newMasterClient);
            }
        }
    }
    
    internal class InGroupCallbacksContainer : List<IInGroupCallbacks>, IInGroupCallbacks
    {
        private readonly RealtimeClient _client;

        public InGroupCallbacksContainer(RealtimeClient client)
        {
            _client = client;
        }
        
        public void OnPlayerEnteredGroup(Player newPlayer)
        {
            _client.UpdateCallbackTargets();

            foreach (IInGroupCallbacks target in this)
            {
                target.OnPlayerEnteredGroup(newPlayer);
            }
        }

        public void OnPlayerLeftGroup(Player otherPlayer)
        {
            _client.UpdateCallbackTargets();

            foreach (IInGroupCallbacks target in this)
            {
                target.OnPlayerLeftGroup(otherPlayer);
            }
        }
    }

    internal class ErrorInfoCallbacksContainer : List<IErrorInfoCallbacks>, IErrorInfoCallbacks
    {
        private RealtimeClient _client;

        public ErrorInfoCallbacksContainer(RealtimeClient client)
        {
            _client = client;
        }
        
        public void OnErrorInfo()
        {
            _client.UpdateCallbackTargets();
            
            foreach (IErrorInfoCallbacks target in this)
            {
                target.OnErrorInfo();
            }
        }
    }
    
    public class ErrorInfo
    {
        /// <summary>
        /// String containing information about the error.
        /// </summary>
        public readonly string Info;

        // public ErrorInfo(EventData eventData)
        // {
        //     this.Info = eventData[ParameterCode.Info] as string;
        // }

        public ErrorInfo(string info)
        {
            Info = info;
        }

        public override string ToString()
        {
            return $"ErrorInfo: {this.Info}";
        }
    }
}