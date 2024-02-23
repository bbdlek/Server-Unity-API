using Protocol;

namespace MVS.Realtime
{
    public class Player
    {
        private PlayerInfo _playerInfo;

        public PlayerInfo PlayerInfo
        {
            get => _playerInfo;
            set => _playerInfo = value;
        }

        public Player (PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }
        
        protected internal RoomInfo RoomReference { get; set; }

        private string nickName = string.Empty;

        public string NickName => _playerInfo.Name;

        public ulong UserId => _playerInfo.PlayerID;
    }
}