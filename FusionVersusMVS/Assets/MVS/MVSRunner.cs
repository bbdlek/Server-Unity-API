using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Protocol;
using UnityEngine;

namespace MVS
{
    [AddComponentMenu("MVS/MVS Runner")]
    [DisallowMultipleComponent]
    public class MVSRunner : Singleton<MVSRunner>
    {
        public string authToken;
        public ulong appID;
        public ulong waplRoomID;
        public string name;

        public uint sceneNumber;
        public uint channelID;

        public bool isRoomJoined = false;
        
        // 서버 접속
        
        private WebSocketHandler _wsh;

        private async Task ConnectToMVS()
        {
            _wsh = new WebSocketHandler(1);
            _wsh.Init();
            var task = Task.Run(() => _wsh.ConnectServer());
            await task;
        }

        public async Task GameStart()
        {
            await ConnectToMVS();
            // Room생성
            authToken = "Token";
            appID = 1;
            waplRoomID = 1;
            name = "TestRoom";
            var pkt = new C_ROOM_JOIN_OR_CREATE
            {
                AuthToken = authToken,
                AppID = appID,
                WaplRoomID = waplRoomID,
                Name = name
            };
            var data = pkt.ToByteArray();
            var size = pkt.CalculateSize();
            _wsh.HandlePacket(WebSocketHandler.PKT_ID.PKT_C_ROOM_JOIN_OR_CREATE, ref data, ref size);

            sceneNumber = 0;
            channelID = 0;
            
            var pktGroup = new C_GROUP_JOIN
            {
                GroupID = new GroupID
                {
                    SceneNumber = sceneNumber,
                    ChannelID = channelID
                }
            };
            var dataG = pkt.ToByteArray();
            var sizeG = pkt.CalculateSize();
            _wsh.HandlePacket(WebSocketHandler.PKT_ID.PKT_C_GROUP_JOIN, ref dataG, ref sizeG);
        }

        public void SendChat()
        {
            var pkt = new C_CHAT
            {
                Msg = "TestChat"
            };
            var data = pkt.ToByteArray();
            var size = pkt.CalculateSize();
            _wsh.HandlePacket(WebSocketHandler.PKT_ID.PKT_C_CHAT, ref data, ref size);
        }

        private void Update()
        {
            _wsh.ProcessReceiveData();
        }
    }
}
