using System.Collections.Generic;
using Protocol;

namespace MVS.Realtime
{
    public class Room
    {
        public RealtimeClient RealtimeClient { get; set; }
        public Room(RoomInfo roomInfo)
        {
            RoomInfo = roomInfo;
        }
        
        private RoomInfo _roomInfo;
        
        public RoomInfo RoomInfo
        {
            get => _roomInfo;
            set => _roomInfo = value;
        }

        private List<Group> _groupList = new List<Group>();

        public List<Group> GroupList => _groupList;

        private Dictionary<ulong, Player> _playerList = new Dictionary<ulong, Player>();

        public Dictionary<ulong, Player> PlayerList => _playerList;

        public Dictionary<ulong, Player> GetPlayerList()
        {
            return PlayerList;
        }

        public virtual Player StorePlayer(Player player)
        {
            PlayerList[player.UserId] = player;
            player.RoomReference = this;

            return player;
        }

        public Player GetPlayer(ulong playerId, bool findMaster = false)
        {
            ulong id = findMaster && playerId == 0 ? MasterClientId : playerId;

            Player result;
            PlayerList.TryGetValue(id, out result);

            return result;
        }

        public ulong masterClientId;

        public ulong MasterClientId => masterClientId;
    }
}