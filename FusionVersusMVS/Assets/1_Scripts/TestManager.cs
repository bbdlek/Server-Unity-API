using System.Collections.Generic;
using MVS.Helios;
using MVS.Realtime;

public class TestManager : MonoBehaviorHeliosCallbacks
{
    public override void OnConnectedToMasterServer() { }

    public override void OnConnected() { }

    public override void OnDisconnected() { }

    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }

    public override void OnCustomAuthenticationFailed(string debugMessage) { }

    public override void OnCreatedRoom() { }

    public override void OnCreatedRoomFailed(string message) { }

    public override void OnJoinedRoom() { }

    public override void OnJoinedRoomFailed(string message) { }

    public override void OnLeftRoom() { }

    public override void OnPlayerEnteredRoom(Player newPlayer) { }

    public override void OnPlayerLeftRoom(Player otherPlayer) { }

    public override void OnPlayerLeftGroup(Player otherPlayer) { }

    public override void OnEvent(EventData eventData) { }

    public override void OnErrorInfo(string errorInfo) { }

    public override void OnMasterClientSwitched(Player newMasterClient) { }

    public override void OnCreatedGroup() { }

    public override void OnCreatedGroupFailed(string message) { }

    public override void OnJoinedGroup() { }

    public override void OnJoinedGroupFailed(string message) { }

    public override void OnLeftGroup() { }

    public override void OnPlayerEnteredGroup(Player newPlayer) { }
}
