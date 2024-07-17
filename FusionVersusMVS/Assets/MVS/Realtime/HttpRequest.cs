using System;

namespace MVS.Realtime
{
    [Serializable]
    public class HttpRequest
    {
        [Serializable]
        public class RoomCreateRequest
        {
            public string roomId;
            public bool isPassword;
            public string name;
        }

        [Serializable]
        public class RoomJoinRequest
        {
            public string roomId;
        }
    }
}