using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
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
        
        WaitNameServerRes,
        WaitMasterServerRes,
        RoomInfoRes,
        
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

    #region Struct

    public struct RoomInfo
    {
        public string Name;
        public ulong RoomID;
    }

    #endregion

    public class RealtimeClient : IRealtimePeerListener
    {
        public RealtimePeer RealtimePeer { get; private set; }

        public string AppVersion { get; set; }

        public string AppId { get; set; }
        public ConnectionProtocol? ConnectionProtocol { get; set; }
        
        public bool IsUsingNameServer { get; set; }
        
        public string NameServerAddress { get; set; }
        
        public string MasterServerAddress { get; set; }
        
        // roominfo from mvm
        public List<RoomInfo> MvsRoomInfos { get; set; }
        public RoomInfo SelectedRoomInfo;
        
        public string MVSAddress { get; set; }
        
        public bool IsDirectToMVS { get; set; }
        
        public RoomJoinInfoStruct RoomJoinInfo;
        
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
                    case ClientState.WaitNameServerRes:
                    case ClientState.WaitMasterServerRes:
                    case ClientState.RoomInfoRes:
                    case ClientState.Authenticating:
                    case ClientState.ConnectingToMVS:
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
            
            // #if SUPPORTED_UNITY
            CustomVariables.Register();
            // #endif
            
            State = ClientState.Created;
            MVSDebug(DebugLevel.INFO, "PeerCreate");
        }

        #region NameServer

        public int NameServerPortInAppSettings;

        #endregion

        #region Operations and Commands

        public virtual async Task<bool> ConnectUsingSettings(AppSettings appSettings)
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
            IsUsingNameServer = appSettings.IsUsingNameServer;
            ConnectionProtocol = appSettings.Protocol;
            NameServerAddress = appSettings.NameServer;
            MasterServerAddress = appSettings.MVM;
            DisconnectedReason = DisconnectedReason.None;

            if (IsUsingNameServer)
            {
                Server = ServerConnection.NameServer;
                State = ClientState.WaitNameServerRes;
                var res = await OpGetMvmAddress();
                if (res==null)
                {
                    return false;
                }
            }
            
            if(IsDirectToMVS)
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
                State = ClientState.WaitMasterServerRes;
                var res = await OpGetRoomList();
            }

            return true;
        }

        #endregion
        
        public async Task<bool> Connect(string serverAddress, string serverPort, string appId, ServerConnection serverType)
        {
            if (State == ClientState.Disconnecting)
            {
                MVSDebug(DebugLevel.ERROR, $"Connect() failed because Current State is {State}");
                return false;
            }

            DisconnectedReason = DisconnectedReason.None;

            switch (serverType)
            {
                case ServerConnection.NameServer:
                case ServerConnection.MasterServer:
                    return false;
                case ServerConnection.MVS:
                    State = ClientState.ConnectingToMVS;
                    break;

            }
            
            bool connecting = RealtimePeer.Connect(serverAddress + ":" + serverPort, appId, serverType);
            if (connecting)
            {
                Server = serverType;
            }

            return connecting;
        }

        public void OnConnected()
        {
            if(!IsDirectToMVS)
                OpCreateOrJoinRoomToMvs(RoomJoinInfo);
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
                case ServerConnection.MasterServer:
                    State = ClientState.DisConnected;
                    return;
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
        
        public virtual bool SendEvent(int eventCode, IMessage fixedData, List<Protocol.HeliosVariable> customData = null)
        {
            return this.RealtimePeer.SendEvent(eventCode, fixedData, customData);
        }

        public DebugLevel AppSettingsDebug = DebugLevel.ERROR;

        public virtual void MVSDebug(DebugLevel debugLevel, string msg)
        {
            if(debugLevel <= AppSettingsDebug)
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
                        case ClientState.ConnectingToMVS:
                            MVSDebug(DebugLevel.INFO, "ConnectingToMVS");
                            State = ClientState.ConnectedToMVS;
                            OnConnected();
                            ConnectionCallbacksTarget.OnConnected();
                            break;
                    }
                    break;
                case StatusCode.Disconnect:
                    switch (State)
                    {
                        case ClientState.Created:
                            break;
                        case ClientState.DisConnectingFromMVS:
                            MVSDebug(DebugLevel.INFO, "DisconnectedFromMVS");
                            break;
                    }

                    // State = ClientState.DisConnected;
                    ConnectionCallbacksTarget.OnDisconnected();
                    break;
                case StatusCode.Exception:
                    break;
                case StatusCode.SendError:
                    break;  
            }
        }

        public long CurRTT = 0;

        public virtual void OnEvent(EventData eventData)
        {
            // Player player = CurrentRoom != null ? CurrentRoom.GetPlayer(eventData.Sender) : null;
            
            switch (eventData.code)
            {
                case EventCode.PKT_S_OTHER_CLIENT_JOINED:
                {
                    var data = Packs.Parser.ParseFrom(eventData.FixedData).SOtherClientGroupJoined;
                    Player otherPlayer = new Player(data.PlayerInfo);
                    CurrentGroup.StorePlayer(otherPlayer);
                    CurrentRoom.StorePlayer(otherPlayer);
                    InGroupCallbacksTarget.OnPlayerEnteredGroup(otherPlayer);
                }   break;
                case EventCode.PKT_S_OTHER_CLIENT_LEAVE:
                {
                    var data = Packs.Parser.ParseFrom(eventData.FixedData).SOtherClientGroupLeave;
                    Player otherPlayer = new Player(data.PlayerInfo);
                    //TODO: Remove Player
                    CurrentGroup.RemovePlayer(otherPlayer);
                    CurrentRoom.RemovePlayer(otherPlayer);
                    InGroupCallbacksTarget.OnPlayerLeftGroup(otherPlayer);
                }   break;
                case EventCode.PKT_S_UPDATE_NETWORK_OBJECTS:
                    //RTT
                    if(eventData.Sender == LocalPlayer.PlayerInfo.PlayerID)
                    {
                        var lastEntry = eventData.CustomData.FindLast(x => x.Key == 10000);
                        if(lastEntry != null)
                        {
                            var sequenceNum = lastEntry.NInt32;
                            if (RealtimePeer.StopwatchList.ContainsKey(sequenceNum))
                            {
                                var sw = RealtimePeer.StopwatchList[
                                    eventData.CustomData.FindLast(x => x.Key == 10000).NInt32];
                                var rtt = sw.ElapsedMilliseconds;
                                CurRTT = rtt;
                                sw.Stop();
                                RealtimePeer.StopwatchList.Remove(sequenceNum);
                            }
                        }
                    }
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
                case OperationCode.AUTHENTICATE:
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
                case OperationCode.ROOM_JOIN_OR_CREATE:
                    JoinRoom(operationResponse);
                    break;
                case OperationCode.GROUP_JOIN:
                    JoinGroup(operationResponse);
                    break;
                case OperationCode.PLAYER_ID:
                    var dataPlayerID = Packs.Parser.ParseFrom(operationResponse.FixedData).SRoomJoinOrCreate;
                    break;
                case OperationCode.ROOM_LIST:
                    /*RoomList.Clear();
                    var dataRoomList = Packs.Parser.ParseFrom(operationResponse.FixedData).SRoomList;
                    if(dataRoomList.RoomInfos.Count > 0)
                    {
                        foreach (var roomInfo in dataRoomList.RoomInfos)
                        {
                            RoomList.Add(new Room(roomInfo));
                        }

                        _roomTask.SetResult(RoomList);
                    }
                    else _roomTask.SetResult(null);*/
                    break;
                case OperationCode.GROUP_LIST:
                    var dataGroupList = Packs.Parser.ParseFrom(operationResponse.FixedData).SGroupList;
                    if(dataGroupList.GroupInfos.Count > 0)
                    {
                        foreach (var groupInfo in dataGroupList.GroupInfos)
                        {
                            CurrentRoom.GroupList.Clear();
                            CurrentRoom.GroupList.Add(new Group(groupInfo, CurrentRoom));
                        }

                        _groupTask.SetResult(CurrentRoom.GroupList);
                    }
                    else _groupTask.SetResult(null);
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
            var data = Packs.Parser.ParseFrom(operationResponse.FixedData).SRoomJoinOrCreate;
            
            if (data.Result == Result.Failed)
            {
                MakingRoomCallbacksTarget.OnCreatedRoomFailed((short)Protocol.Result.Failed, "RoomCreateFailed");
                MakingRoomCallbacksTarget.OnJoinedRoomFailed((short)Protocol.Result.Failed, "RoomJoinFailed");
                return;
            }
            
            CurrentRoom = CreateRoom(SelectedRoomInfo);
            CurrentRoom.RealtimeClient = this;
            CurrentRoom.StorePlayer(LocalPlayer);

            State = ClientState.JoinedRoom;
            if (data.Result == Result.SuccessRoomCreate)
            {
                MakingRoomCallbacksTarget.OnCreatedRoom();
                MakingRoomCallbacksTarget.OnJoinedRoom();
            }
            else if (data.Result == Result.SuccessRoomJoined)
            {
                MakingRoomCallbacksTarget.OnJoinedRoom();
            }
            
        }

        protected internal virtual Room CreateRoom(RoomInfo roomInfo)
        {
            Room newRoom = new Room(roomInfo);
            return newRoom;
        }

        private void JoinGroup(OperationResponse operationResponse)
        {
            /*var pkt = new C_PLAYER_ID();
            RealtimePeer.SendOperation(Protocol.OperationCode.PlayerId, pkt);*/
            
            var data = Packs.Parser.ParseFrom(operationResponse.FixedData).SGroupJoin;
            GroupInfo groupInfo = data.GroupInfo;
            Group newGroup = new Group(groupInfo, CurrentRoom);

            CurrentGroup = newGroup;
            foreach (var playerInfo in newGroup.GroupInfo.PlayerInfos)
            {
                Player existedPlayer = new Player(playerInfo);
                CurrentGroup.StorePlayer(existedPlayer);
                CurrentRoom.StorePlayer(existedPlayer);
            }
            CurrentGroup.RealtimeClient = this;

            State = ClientState.JoinedGroup;
            if (data.Result == Result.SuccessGroupCreate)
            {
                MakingGroupCallbacksTarget.OnCreatedGroup();
                MakingGroupCallbacksTarget.OnJoinedGroup();
            }
            else if(data.Result == Result.SuccessGroupJoined)
            {
                MakingGroupCallbacksTarget.OnJoinedGroup();
            }

            if (data.Result >= Result.FailedGroupNotExistsGroup && data.Result <= Result.FailedGroupAlreadyExistsPlayer)
            {
                MakingRoomCallbacksTarget.OnCreatedRoomFailed((short)data.Result, data.Result.ToString());
                MakingRoomCallbacksTarget.OnJoinedRoomFailed((short)data.Result, data.Result.ToString());
            }
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
        
        ///Functions
        
        public bool OpRoomList()
        {
            // Lagacy
            if (!CheckOpCanBeSent((byte)OperationCode.ROOM_LIST, Server, "RoomList"))
            {
                return false;
            }

            bool sent = RealtimePeer.OpRoomList();

            return sent;
        }

        static HttpModule httpModule = new HttpModule();

        public async Task<string> OpGetMvmAddress()
        {
            if (IsUsingNameServer)
            {
                var res = await httpModule.GetAsync($"http://{NameServerAddress}/ns/api/v1/app/{AppId}");
                if (res == null)
                {
                    MVSDebug(DebugLevel.ERROR, "Get MVM Address request fail");
                    return null;
                }
                var res2 = JsonConvert.DeserializeObject<MVMResponse>(res);
                MVSDebug(DebugLevel.INFO, res2.ResponseMessage[0].Hostname.ToString());
                MasterServerAddress = res2.ResponseMessage[0].IpAddress;
            
                return MasterServerAddress;
            }
            else
            {
                return MasterServerAddress;
            }
        }
        
        /// <summary>
        /// Send RoomList Req to MVM
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoomInfo>> OpGetRoomList()
        {
            var res = await httpModule.GetAsync($"http://{MasterServerAddress}/mvm/api/rooms");
            if (res == null)
            {
                MVSDebug(DebugLevel.ERROR, "GetRoomList Request Fail");
                return null;
            }
            var res2 = JsonConvert.DeserializeObject<RoomListResponse>(res);
            MVSDebug(DebugLevel.INFO, res2.ResponseMessage.ToString());
            MvsRoomInfos = new List<RoomInfo>();
            foreach (var roomRes in res2.ResponseMessage)
            {
                MvsRoomInfos.Add(
                    new RoomInfo()
                    {
                        RoomID = roomRes.RoomId,
                        Name = roomRes.RoomName
                    }
                );

                MVSDebug(DebugLevel.INFO,($"room id : {roomRes.RoomId}, ip : {roomRes.Url}, name : {roomRes.RoomName}"));
            }

            if (MvsRoomInfos.Count > 0)
            {
                SelectedRoomInfo = MvsRoomInfos[0];
            }

            return MvsRoomInfos;
        }
        
        /// <summary>
        /// MVM에 룸 생성 요청.
        /// RoomId가 0일시 서버에서 임의로 결정
        /// 현재 Password는 구현되어 있지 않음.
        /// </summary>
        /// <returns></returns>
        public async Task<RoomJoinInfoStruct> OpCreateAndJoinRoomToMvm(RoomInfo roomInfo = default, bool isPassword = false)
        {
            HttpRequest.RoomCreateRequest roomReq;
            roomReq = new HttpRequest.RoomCreateRequest()
            {
                RoomId = roomInfo.RoomID.ToString(),
                Name = roomInfo.Name,
                IsPassword = isPassword
            };
            
            var res = await httpModule.PostAsync<HttpRequest.RoomCreateRequest, RoomCreateResponse>(
                $"http://{MasterServerAddress}/mvm/api/rooms", roomReq);
            if (res == null)
            {
                MVSDebug(DebugLevel.ERROR, "Room Create Req Fail");
                return default;
            }
            
            // 이미 방이 만들어 져서 join response가 온경우
            if (!res.ResponseMessage.CreationFlag)
            {
                MVSDebug(DebugLevel.WARNING, "Already Created RoomId");
                return default;
            }
            
            var address = res.ResponseMessage.MvsUrl.Split('/');
            RoomJoinInfo.IP = address[0];
            RoomJoinInfo.Port = address[1].Replace("mvs", "3000");
            RoomJoinInfo.RoomID = res.ResponseMessage.RoomId;
            RoomJoinInfo.MvsUserID = res.ResponseMessage.UserId;
            LocalPlayer.PlayerInfo.PlayerID = RoomJoinInfo.MvsUserID;
            RoomJoinInfo.MvsUserToken = res.ResponseMessage.Token;
            
            await Connect(RoomJoinInfo.IP, RoomJoinInfo.Port, AppId, ServerConnection.MVS);

            return RoomJoinInfo;
        }

        public async Task<RoomJoinInfoStruct> OpJoinRoomToMvm(UInt64 roomId = default)
        {
            HttpRequest.RoomJoinRequest roomReq;
            if (roomId == 0)
            {
                // Use SelectedRoomInfo
                roomReq = new HttpRequest.RoomJoinRequest()
                {
                    RoomId = SelectedRoomInfo.RoomID.ToString()
                };
            }
            else
            {
                roomReq = new HttpRequest.RoomJoinRequest()
                {
                    RoomId = roomId.ToString()
                };
            }
            
            var res = await httpModule.PostAsync<HttpRequest.RoomJoinRequest, RoomJoinResponse>(
                $"http://{MasterServerAddress}/mvm/api/rooms", roomReq);
            if (res == null)
            {
                MVSDebug(DebugLevel.ERROR, "Room Join Req Fail");
                return default;
            }
            
            // join request를 보냈는데 create response가 온경우
            if (res.ResponseMessage.CreationFlag)
            {
                MVSDebug(DebugLevel.WARNING, "Join Request but Room Created");
                return default;
            }

            var address = res.ResponseMessage.MvsUrl.Split('/');
            RoomJoinInfo.IP = address[0];
            RoomJoinInfo.Port = address[1].Replace("mvs", "3000");
            RoomJoinInfo.RoomID = res.ResponseMessage.RoomId;
            RoomJoinInfo.MvsUserID = res.ResponseMessage.UserId;
            RoomJoinInfo.MvsUserToken = res.ResponseMessage.Token;

            LocalPlayer.PlayerInfo.PlayerID = res.ResponseMessage.UserId;

            if(state!= ClientState.ConnectingToMVS)
                Connect(RoomJoinInfo.IP, RoomJoinInfo.Port, AppId, ServerConnection.MVS);
            
            return RoomJoinInfo;
        }
        
        public bool OpCreateOrJoinRoomToMvs(RoomJoinInfoStruct joinRoomParams)
        {
            if (!CheckOpCanBeSent((byte)OperationCode.ROOM_JOIN_OR_CREATE, Server, "CreateRoom"))
            {
                return false;
            }

            bool sent = RealtimePeer.OpCreateRoom(joinRoomParams);

            return sent;
        }
        
        private TaskCompletionSource<List<Group>> _groupTask;

        public async Task OpGroupTask()
        {
            _groupTask = new TaskCompletionSource<List<Group>>();
            RealtimePeer.OpGroupList();
            await _groupTask.Task;
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
            if (!CheckOpCanBeSent((byte)OperationCode.GROUP_JOIN, Server, "JoinGroup"))
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

        public bool OpInitVariables()
        {
            /*var InitVariablesPkt = new C_INIT_VARIABLES();
            foreach (var pair in CustomVariables.VarDic)
            {
                InitVariablesPkt.Variables.Add(pair.Key, pair.Value);
            }

            bool sent = RealtimePeer.OpInitVariables(InitVariablesPkt);

            return sent;*/

            return true;
        }
        
        public virtual bool OpRaiseEvent(int EventCode, IMessage pkt = null, List<Protocol.HeliosVariable> customData = null)
        {
            if (!CheckOpCanBeSent((byte)OperationCode.RAISE_EVENT, Server, "RaiseEvent"))
            {
                return false;
            }

            return RealtimePeer.SendEvent(EventCode, pkt, customData);
        }
    }

    public interface IConnectionCallbacks
    {
        void OnConnected();

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
    }
    
    public interface IInGroupCallbacks
    {
        void OnPlayerEnteredGroup(Player newPlayer);
        
        void OnPlayerLeftGroup(Player otherPlayer);
        
        void OnMasterClientSwitched(Player newMasterClient);
    }

    public interface IOnEventCallbacks
    {
        void OnEvent(EventData eventData);
    }

    public interface IErrorInfoCallbacks
    {
        void OnErrorInfo(string errorInfo);
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
        
        public void OnMasterClientSwitched(Player newMasterClient)
        {
            _client.UpdateCallbackTargets();

            foreach (IInGroupCallbacks target in this)
            {
                target.OnMasterClientSwitched(newMasterClient);
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
        
        public void OnErrorInfo(string errorInfo)
        {
            _client.UpdateCallbackTargets();
            
            foreach (IErrorInfoCallbacks target in this)
            {
                target.OnErrorInfo(errorInfo);
            }
        }
    }
}