using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

public class PropertyAccessorDetourExample : MonoBehaviour
{
    private TestData _data;

    private delegate void SetStringValue(string value);

    private void Start()
    {
        var fields = typeof(TestData).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            Debug.Log(field.Name);
        }

        return;
        var type = typeof(TestData);
        var propertyInfo = type.GetProperty(nameof(TestData.StringValue), BindingFlags.Public | BindingFlags.Instance);
        var setMethod = propertyInfo.GetSetMethod();

        /*var dynamicMethod = new DynamicMethod(
            "SetValueDetour",
            typeof(void),
            new[] {typeof(string)},
            typeof(PropertyAccessorDetourExample).Module);

        var il = dynamicMethod.GetILGenerator(256);
        // il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldarg_0);
        // il.Emit(OpCodes.Ldarg_1);
        il.EmitCall(OpCodes.Call, setMethod, null);
        il.Emit(OpCodes.Ret);

        var replaceDelegate = dynamicMethod.CreateDelegate(typeof(SetStringValue));

        Memory.DetourMethod(setMethod, replaceDelegate.Method);*/

        /*2*/
        var method = typeof(PropertyAccessorDetourExample).GetMethod(
            nameof(OnSetStringValue),
            BindingFlags.NonPublic | BindingFlags.Instance);

        Memory.DetourMethod(setMethod, method);

        _data = new TestData();
    }

    private void OnSetStringValue(string value)
    {
        /*当此方法detour到属性的set方法后,this将指向TestData*/
        Debug.Log($"on set {value}, {this.GetType().Name}");
    }


    private static void PostSetValue<T>(T value)
    {
        Debug.Log($"PostSetValue {value}");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("set value"))
        {
            _data.StringValue = "66";
            Debug.Log(_data.StringValue);
        }
    }

    private void NullMethod()
    {
    }
}

public class TestData
{
    public string StringValue { get; set; }
}