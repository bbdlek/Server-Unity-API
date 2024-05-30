using System.Collections.Generic;
using Protocol;

namespace MVS.Realtime
{
    public class Group
    {
        public RealtimeClient RealtimeClient { get; set; }
        
        public GroupInfo GroupInfo { get; set; }

        public Group(GroupInfo groupInfo, Room roomReference)
        {
            RoomReference = roomReference;
            GroupInfo = groupInfo;
        }
        
        public Room RoomReference { get; set; }
        
        private Dictionary<ulong, Player> _playerList = new Dictionary<ulong, Player>();

        public Dictionary<ulong, Player> PlayerList => _playerList;

        public Dictionary<ulong, Player> GetPlayerList()
        {
            return PlayerList;
        }

        public virtual void StorePlayer(Player player)
        {
            PlayerList[player.UserId] = player;
            player.GroupReference = this;
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