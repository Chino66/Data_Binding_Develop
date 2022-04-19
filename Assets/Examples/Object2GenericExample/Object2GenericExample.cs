using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object2GenericExample : MonoBehaviour
{
    private void Start()
    {
        StartExample();
    }


    private void StartExample()
    {
        
    }
}

public class MyGeneric<T>
{
    public string Name;
    private T _value;
    public Action<T> OnGet;
    public Action<T> OnSet;

    public T Value
    {
        get => Get();
        set => Set(value);
    }

    public T Get()
    {
        OnGet?.Invoke(_value);
        return _value;
    }

    public void Set(T value)
    {
        _value = value;
        OnSet?.Invoke(_value);
    }
}

public class MyData
{
    public string StringValue;
    public int IntValue;
    public bool BoolValue;
}