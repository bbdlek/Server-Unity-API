using System;
using PostSharp.Aspects;
using PostSharp.Serialization;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class NetworkedAttribute : Attribute
{
}

[PSerializable]
public class NetworkedAspect : OnMethodBoundaryAspect
{
    public override void OnEntry(MethodExecutionArgs args)
    {
        Debug.Log("Method entry: " + args.Method.Name);
    }

    public override void OnExit(MethodExecutionArgs args)
    {
        Debug.Log("Method exit: " + args.Method.Name);
    }
}

public class HeliosAttribute : MonoBehaviour
{
    [SerializeField]
    private int age;
    
    [NetworkedAspect] 
    public int Age { get => age; set => age = value; }

    private void Start()
    {
        // 값을 설정할 때 로그 출력
        Age = 3; // Setting value: 3

        // 값을 가져올 때 로그 출력
        int ageValue = Age; // Getting value: 3
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Age++;
        }
    }
}