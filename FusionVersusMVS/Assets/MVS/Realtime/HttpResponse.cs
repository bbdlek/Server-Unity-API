using System;
using System.Collections.Generic;

namespace MVS.Realtime
{
    public class MVMResponse
    {
        public List<MVMRes> ResponseMessage { get; set; }
        public DateTime Timestamp { get; set; }
        
    }

    public class MVMRes
    {
        public string Hostname { get; set; }
        public string RegionCode { get; set; }
        public string IpAddress { get; set; }

        public override string ToString()
        {
            return $"Hostname : {Hostname}, RegionCode : {RegionCode}, IpAddress : {IpAddress}";
        }
    }
    
    public class RoomListResponse
    {
        public List<RoomRes> ResponseMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RoomRes
    {
        public int RoomId { get; set; }
        public string Url { get; set; }
        public bool IsPending { get; set; }
        public string RoomName { get; set; }

        public override string ToString()
        {
            return $"RoomId : {RoomId}, Url : {Url}, IsPending : {IsPending}, RoomName : {RoomName}";
        }
    }

    public class RoomCreateResponse
    {
        public string ResponseMessage { get; set; }

        public override string ToString()
        {
            return $"Response : {ResponseMessage}";
        }
    }

    public class RoomJoinResponse
    {
        public string ResponseMessage { get; set; }
        
        public override string ToString()
        {
            return $"Response : {ResponseMessage}";
        }
    }
}