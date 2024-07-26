using System;
using MVS.Helios;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubeManager : HeliosMonoBehavior
{
    [HeliosRPC]
    public void AddScore()
    {
        Debug.Log("AddScore");
        score++;
    }

    public int score = 1;
    [HNSync, OnChanged(nameof(OnChangeScore2))] public int score2 = 3;

    private void OnChangeScore2()
    {
        Debug.Log($"OnChangeScore2 {score2}");
    }

    private Animator _animator;
    
    public override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        Debug.Log(IsMine);
        if(IsMine)
            RPC("AddScore");
    }

    private void OnEnable()
    {
        Debug.Log("Im Enbled");
    }

    private void Start()
    {
        if(IsMine)
            RPC("AddScore");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && IsMine)
        {
            RPC("AddScore");
        }
        
        if (Input.GetKeyDown(KeyCode.L) && IsMine)
        {
            score2++;
        }
        
        
        //Animator Test
        if (Input.GetKeyDown(KeyCode.A))
        {
            _animator.SetBool("TestBool", !_animator.GetBool("TestBool"));
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            _animator.SetTrigger("TestTrigger");
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            _animator.SetFloat("TestFloat", Random.Range(1f, 10f));
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            _animator.SetInteger("TestInt", Random.Range(1, 10));
        }
    }
    

}
