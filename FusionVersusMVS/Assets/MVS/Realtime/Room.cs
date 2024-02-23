using System.Collections.Generic;
using Protocol;

namespace MVS.Realtime
{
    public class Room
    {
        private RoomInfo _roomInfo;
        
        public RoomInfo RoomInfo => _roomInfo;

        private List<Player> _playerList;

        public List<Player> PlayerList => _playerList;

        public List<Player> GetPlayerList()
        {
            return PlayerList;
        }

        public void AddPlayer(PlayerInfo playerInfo)
        {
            PlayerList.Add(new Player(playerInfo));
        }
    }
}