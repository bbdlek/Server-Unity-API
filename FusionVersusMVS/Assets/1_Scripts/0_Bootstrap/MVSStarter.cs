using System.Collections;
using System.Collections.Generic;
using MVS;
using Unity.VisualScripting;
using UnityEngine;

public class MVSStarter : MonoBehaviour
{
    private MVSRunner _runner;

    public async void StartGame()
    {
        _runner = gameObject.GetOrAddComponent<MVSRunner>();

        await _runner.GameStart();
    }

    public void SendChatTest()
    {
        _runner.SendChat();
    }
}
