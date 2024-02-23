using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;

public interface MVSCallbacks
{
    void OnRoomJoined();

    void OnGroupJoined();

    void OnPlayerJoined(PlayerInfo playerInfo);

    void OnPlayerLeft(PlayerInfo playerInfo);

    void OnInput();
}
