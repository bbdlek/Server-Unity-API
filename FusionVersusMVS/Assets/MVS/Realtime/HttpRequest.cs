namespace MVS.Realtime
{
    public class HttpRequest
    {
        public class RoomCreateRequest
        {
            public string RoomId { get; set; }
            public bool IsPassword { get; set; }
            public string Name { get; set; }
        }

        public class RoomJoinRequest
        {
            public string RoomId { get; set; }
        }
    }
}