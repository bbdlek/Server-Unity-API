using System;
using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using MVS.Helios.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : HeliosMonoBehavior
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public Animator animator;

    private CharacterController controller;
    private Vector3 movement;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if(!IsMine) return;
        // 입력 처리
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movement = new Vector3(horizontal, 0f, vertical).normalized;

        // Animator의 Speed 파라미터 설정
        animator.SetFloat("Speed", movement.magnitude);

        if (movement.magnitude > 0.1f)
        {
            // 이동 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if(!IsMine) return;
        // 물리 이동 처리
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }
}
