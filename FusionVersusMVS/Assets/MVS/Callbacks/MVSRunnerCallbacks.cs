using System.Collections;
using System.Collections.Generic;
using MVS;
using Protocol;
using UnityEngine;

public interface MVSRunnerCallbacks
{
    void OnRoomJoined(MVSRunner runner);

    void OnGroupJoined(MVSRunner runner);

    void OnPlayerJoined(MVSRunner runner, PlayerInfo playerInfo);

    void OnPlayerLeft(MVSRunner runner, PlayerInfo playerInfo);

    void OnInput(MVSRunner runner);

    void OnShutDown(MVSRunner runner);

    void OnConnectedToServer(MVSRunner runner);

    void OnGroupListUpdated(MVSRunner runner);

    void OnDataReceived(MVSRunner runner);
}
