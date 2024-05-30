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
        
        protected internal Room RoomReference { get; set; }
        
        protected internal Group GroupReference { get; set; }

        public readonly bool IsLocal;

        private string nickName = string.Empty;

        public string NickName => _playerInfo.Name;

        public ulong UserId => _playerInfo.PlayerID;

        public override bool Equals(object p)
        {
            Player player = p as Player;
            return player != null;
        }
    }
}