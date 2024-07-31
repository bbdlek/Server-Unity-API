using System.Collections.Generic;
using Protocol;

namespace MVS.Realtime
{
    public class Group
    {
        public RealtimeClient RealtimeClient { get; set; }
        
        internal GroupInfo GroupInfo { get; set; }

        public uint SceneNumber => GroupInfo.GroupID.SceneNumber;

        public uint ChannelID => GroupInfo.GroupID.ChannelID;
        
        public bool IsLocalGroupOwner { get; set; }

        internal Group(GroupInfo groupInfo, Room roomReference)
        {
            RoomReference = roomReference;
            GroupInfo = groupInfo;
            IsLocalGroupOwner = false;
        }
        
        public Room RoomReference { get; set; }
        
        private Dictionary<ulong, Player> _playerList = new Dictionary<ulong, Player>();

        public Dictionary<ulong, Player> PlayerList => _playerList;

        public Dictionary<ulong, Player> GetPlayerList()
        {
            return PlayerList;
        }

        internal virtual void StorePlayer(Player player)
        {
            PlayerList[player.UserId] = player;
            player.GroupReference = this;
        }

        internal virtual void RemovePlayer(Player player)
        {
            if (PlayerList.ContainsKey(player.UserId))
            {
                PlayerList.Remove(player.UserId);
            }
        }

        public Player GetPlayer(ulong playerId, bool findMaster = false)
        {
            ulong id = findMaster && playerId == 0 ? MasterClientId : playerId;

            Player result;
            PlayerList.TryGetValue(id, out result);

            return result;
        }

        internal ulong masterClientId;

        internal ulong MasterClientId => masterClientId;
    }
}