using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateExample : MonoBehaviour
{
    private Action<string> action;

    private Action<string>[] _actions;

    void Start()
    {
        _actions = new Action<string>[3];

        _actions[0] = (value) => { Debug.Log($"1 value is {value}"); };
        _actions[1] = (value) => { Debug.Log($"2 value is {value}"); };
        _actions[2] = (value) => { Debug.Log($"3 value is {value}"); };

        action += _actions[0];

        action += _actions[1];

        action += _actions[2];
    }

    private void OnGUI()
    {
        if (GUILayout.Button("invoke action"))
        {
            action("666");
        }

        if (GUILayout.Button("remove action"))
        {
            action -= _actions[0];
        }
    }
}