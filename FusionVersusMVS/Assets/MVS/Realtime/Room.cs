using System.Collections.Generic;

namespace MVS.Realtime
{
    public class Room
    {
        internal RealtimeClient RealtimeClient { get; set; }
        internal Room(RoomInfo roomInfo)
        {
            RoomInfo = roomInfo;
        }

        internal RoomInfo RoomInfo { get; private set; }

        private List<Group> _groupList = new List<Group>();

        public List<Group> GroupList => _groupList;

        public string RoomName => RoomInfo.Name;

        public ulong RoomID => RoomInfo.RoomID;

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

        public virtual void RemovePlayer(Player player)
        {
            if (PlayerList.ContainsKey(player.UserId))
            {
                PlayerList.Remove(player.UserId);
            }
        }

        public Player GetPlayer(ulong playerId)
        {
            Player result;
            PlayerList.TryGetValue(playerId, out result);

            return result;
        }
    }
}