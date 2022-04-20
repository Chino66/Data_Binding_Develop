using System.Reflection;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// 改道方法示例
/// 将方法指针转向目标方法实现方法的改道
/// </summary>
public class DetourMethodExample : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("detour Method"))
        {
            Detour();
        }

        if (GUILayout.Button("invoke MethodA"))
        {
            MethodA();
        }

        if (GUILayout.Button("invoke MethodB"))
        {
            MethodB();
        }
    }

    private void Detour()
    {
        var ma = typeof(DetourMethodExample).GetMethod("MethodA", BindingFlags.Instance | BindingFlags.Public);
        var mb = typeof(DetourMethodExample).GetMethod("MethodB", BindingFlags.Instance | BindingFlags.Public);

        Memory.DetourMethod(ma, mb);

        /*等价于*/
        /*Exception exception;
        long methodAStart = Memory.GetMethodStart(ma, out exception);
        long methodBStart = Memory.GetMethodStart(mb, out exception);
        Memory.WriteJump(methodAStart, methodBStart);*/
    }


    public void MethodA()
    {
        Debug.Log($"this is {nameof(MethodA)}");
    }

    public void MethodB()
    {
        Debug.Log($"this is {nameof(MethodB)}");
    }
}