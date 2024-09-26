using System;
using Protocol;

namespace MVS.Realtime
{
    public class Player
    {
        internal PlayerInfo PlayerInfo { get; }

        internal Player (PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }
        
        protected internal Room RoomReference { get; set; }
        
        protected internal Group GroupReference { get; set; }

        private readonly string _nickName = string.Empty;

        public string NickName => PlayerInfo.Name;

        public ulong UserId => PlayerInfo.PlayerID;

        public override bool Equals(object p)
        {
            Player player = p as Player;
            return player != null;
        }
    }
}