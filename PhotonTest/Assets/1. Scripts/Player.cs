using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    private Rigidbody _rigidbody;
    [SerializeField] private Ball prefabBall;
    [SerializeField] private PhysxBall prefabPhysxBall;
    private Vector3 _forward = Vector3.forward;
    [Networked] private TickTimer delay { get; set; }
    [Networked] public bool spawned { get; set; }
    public Material _material;

    [SerializeField] private float speed;

    private ChangeDetector _changeDetector;

    private void Awake()
    {
        // _cc = GetComponent<NetworkCharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    #region MESSAGE

    private TMP_Text _messages;
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, Channel = RpcChannel.Reliable)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }

        _messages.text = message;
        Debug.Log(message);
    }

    #endregion

    public override void FixedUpdateNetwork()
    {
        if(Runner.GameMode == GameMode.Shared)
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            // _cc.Move(move);
            _rigidbody.AddForce(move * speed);
            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }
        }
        else
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _cc.Move(5*data.direction*Runner.DeltaTime);
            
                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;
            
                if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
                {
                    if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                    {
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                        Runner.Spawn(prefabBall,
                            transform.position + _forward, Quaternion.LookRotation(_forward),
                            Object.InputAuthority, (runner, o) =>
                            {
                                o.GetComponent<Ball>().Init();
                            });
                        spawned = !spawned;
                    }
                    else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                    {
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                        Runner.Spawn(prefabPhysxBall,
                            transform.position + _forward, Quaternion.LookRotation(_forward),
                            Object.InputAuthority, (runner, o) =>
                            {
                                o.GetComponent<PhysxBall>().Init(10 * _forward);
                            });
                    }
                }
            }   
        }
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawned):
                    _material.color = Color.white;
                    break;
            }
        }

        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }
}
