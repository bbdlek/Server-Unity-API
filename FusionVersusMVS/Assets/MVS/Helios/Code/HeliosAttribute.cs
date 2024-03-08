using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class NetworkedAttribute : Attribute
{
    public void OnBeforeSetValue(object value)
    {
        Debug.Log($"Setting value: {value}");
    }

    public void OnAfterGetValue(object value)
    {
        Debug.Log($"Getting value: {value}");
    }
}

public class HeliosAttribute : MonoBehaviour
{
    [Networked] 
    public int Age { get; set; }

    private void Start()
    {
        // 값을 설정할 때 로그 출력
        Age = 3; // Setting value: 3

        // 값을 가져올 때 로그 출력
        int ageValue = Age; // Getting value: 3
    }
}