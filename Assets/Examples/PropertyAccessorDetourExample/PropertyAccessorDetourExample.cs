using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// 属性访问器改道方案探究
/// </summary>
public class PropertyAccessorDetourExample : MonoBehaviour
{
    private TestData _data;

    private delegate void SetStringValue(string value);

    private void Start()
    {
        /*var fields = typeof(TestData).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            Debug.Log(field.Name);
        }

        return;*/

        _data = new TestData();

        /*
         * 尝试1 动态方法改道属性方法,动态方法调用自定义方法
         * 结果:动态方法的使用不正确,待探究
         * 原因是动态方法的参数不对,必须填上调用的实例类在参数的第一个位置
         */
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

        /*
         * 尝试2 直接改道自定义方法
         * 可行,但没有实际意义
         */
        /*var type = typeof(TestData);
        var propertyInfo = type.GetProperty(nameof(TestData.StringValue), BindingFlags.Public | BindingFlags.Instance);
        var setMethod = propertyInfo.GetSetMethod();
        var method = typeof(PropertyAccessorDetourExample).GetMethod(
            nameof(OnSetStringValue),
            BindingFlags.NonPublic | BindingFlags.Instance);
        Memory.DetourMethod(setMethod, method);*/

        /*
         * 尝试3 创建lambda函数,并获取MethodInfo,进行方法改道
         * 尝试用闭包的方式实现外部属性名的参数传递
         * 结果:此方式无法实现闭包,例子中lambda不能正确访问"propertyName"
         * 如果是正常闭包,即没有改道调用lambda,则是可以正确闭包,访问到"propertyName"变量
         * 原因是方法的所有权(者)不对,方法所有者应当指向数据实例类
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;
            var setMethod = propertyInfo.GetSetMethod();
            Action<string> action = (value) =>
            {
                /*当改道后,propertyName不能访问,闭包失效#1#
                Debug.Log($"propertyName is {propertyName}, value is {value}");
            };
            Memory.DetourMethod(setMethod, action.Method);
        }*/

        /*
         * 关于闭包和外部变量:
         * 闭包的本质是在编译时将lambda函数编译成一个nested修饰的类,外部变量在闭包中变成这个类的一个字段
         * 所以闭包时的值就是就是外部变量当时的值
         * 细节查看ClosureExample的IL代码
         */

        /*
         * 尝试4 用Harmony的方式改道,查看闭包问题
         * 结果:Harmony只能使用静态方法进行注入,故不能实现闭包改道
         */
        /*var harmony = new Harmony("com.chino.data.binding.patch");
        var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var setMethod = propertyInfo.GetSetMethod();
            var preMethod = typeof(PropertyAccessorDetourExample).GetMethod(nameof(PreMethod),
                BindingFlags.Public | BindingFlags.Static);
            var postMethod = typeof(PropertyAccessorDetourExample).GetMethod(nameof(PostMethod),
                BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(setMethod, new HarmonyMethod(preMethod), new HarmonyMethod(postMethod));
        }*/

        /*
         * 尝试3 进一步探究
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;
            var setMethod = propertyInfo.GetSetMethod();
            Action<string> action = (value) =>
            {
                var pn = propertyName;
                // 当改道后,this指向"TheData"
                Debug.Log($"this type is {this.GetType().Name}");
                Debug.Log($"value is {value}");
                // 当要输出pn时,this将报空,因为pn不在"TheData"中,而是lambda生成的类中
                Debug.Log($"propertyName is {pn}");
            };
            Memory.DetourMethod(setMethod, action.Method);
        }*/

        /*
         * 尝试5 改道访问外部变量的方法
         * 结果:"Test5Field"报空,原因就是因为this的指向一样了
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var setMethod = propertyInfo.GetSetMethod();
            var method = this.GetType().GetMethod(nameof(Test5), BindingFlags.Public | BindingFlags.Instance);
            Memory.DetourMethod(setMethod, method);
        }*/

        /*
         * 尝试5 进一步探究 如果"Test5Field"是静态变量,将会如何?
         * 结果:"Test5Field"可以访问到
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var setMethod = propertyInfo.GetSetMethod();
            var method = this.GetType().GetMethod(nameof(Test5), BindingFlags.Public | BindingFlags.Instance);
            Memory.DetourMethod(setMethod, method);
        }*/

        /*
         * 尝试5的基础上 对尝试3再次探究 访问静态变量字典
         * 悖论:想要访问字典,必须知道key,key也是外部变量...
         * 探究的方向是,外部变量的所有权在哪儿?
         *  当方法改道后,方法指针在数据类,而外部变量则在注入类中,导致了访问丢失
         *  如果改道后,内部方法的所有权还在注入类,那么就可以访问注入类的外部外部变量了
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;
            var setMethod = propertyInfo.GetSetMethod();
            Test3Dic[propertyName] = propertyName;
            Action<string> action = (value) =>
            {
                Debug.Log($"value is {value}");
                Debug.Log($"propertyName is {Test3Dic[propertyName]}");
            };
            Memory.DetourMethod(setMethod, action.Method);
        }*/

        /*
         * 原型:
         * property.set = (value) => { this.value = value}
         * 
         * 期望:
         * property.set = (value) => // 改道方法的所有权在数据类中
         * {
         *      origin_method(value) // origin_method是原始方法,没有变化
         *      fix_method(value) // fix_method是注入方法,期望它的所有权在注入类
         * }
         *
         * fix_method(value) => { context; } // context是注入类的外部变量,由于fix_method的所有权在注入类,所以可以访问外部变量
         * 目标:
         *      在注入类创建fix_method方法,所有权在注入类
         *      在数据类方法调用fix_method方法
         */

        /*
         * 尝试6 重新理解DynamicMethod后,将action生成委托并改道
         * 结果:改道成功,并且this.GetType()正确输出"TestData"但依然不能拿到"propertyName"的值
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;
            var setMethod = propertyInfo.GetSetMethod();

            /*尝试将它的所有权放在注入类#1#
            Action<string> action = (value) =>
            {
                Debug.Log($"type is {this.GetType()}");
                Debug.Log($"value is {value}");
            };

            var dynamicMethod = new DynamicMethod($"{propertyName}_set",
                typeof(void),
                new[] {typeof(TestData), typeof(string)},
                typeof(TestData));

            var il = dynamicMethod.GetILGenerator(256);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, action.Method, null);
            il.Emit(OpCodes.Ret);

            var dmd = (Action<string>) dynamicMethod.CreateDelegate(typeof(Action<string>), _data);

            Memory.DetourMethod(setMethod, dmd.Method);
        }*/

        /*
         * 尝试7 
         */
        var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;

            /*origin*/
            var originMethod = propertyInfo.GetSetMethod();

            /*fix*/
            Action<string> fixAction = (value) =>
            {
                Debug.Log($"type is {this.GetType()}");
                Debug.Log($"value is {value}");
            };

            var fixDynamicMethod = new DynamicMethod($"fix",
                typeof(void),
                new[] {typeof(TestData), typeof(string)},
                typeof(TestData));

            var il = fixDynamicMethod.GetILGenerator(256);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, fixAction.Method, null);
            il.Emit(OpCodes.Ret);

            var fixmd = (Action<TestData, string>) fixDynamicMethod.CreateDelegate(typeof(Action<TestData, string>));

            /*new*/
            var newDynamicMethod = new DynamicMethod($"new",
                typeof(void),
                new[] {typeof(TestData), typeof(string)},
                typeof(TestData));

            il = newDynamicMethod.GetILGenerator(256);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, fixAction.Method, null);
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldarg_1);
            // il.EmitCall(OpCodes.Call, fixmd.Method, null);
            il.Emit(OpCodes.Ret);

            var newmd = (Action<TestData, string>) newDynamicMethod.CreateDelegate(typeof(Action<TestData, string>));

            Memory.DetourMethod(originMethod, newmd.Method);
        }
    }

    public static Dictionary<string /*property name*/, string /*value*/> Test3Dic = new Dictionary<string, string>();

    public string TestValue = "this is Example value";

    public static string Test5Field = "Test5Field";

    public void Test5(string value)
    {
        Debug.Log($"value is {value}");
        Debug.Log($"Test5Field is {Test5Field}");
        Debug.Log($"TestValue is {TestValue}");
    }

    private void OnSetStringValue(string value)
    {
        /*当此方法detour到属性的set方法后,this将指向TestData*/
        Debug.Log($"on set {value}, {this.GetType().Name}");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("set StringValue"))
        {
            _data.StringValue = "66";
            Debug.Log(_data.StringValue);
        }
    }
}

public class TestData
{
    public string StringValue { get; set; }

    public string TestValue = "this is TestData value";

    public override string ToString()
    {
        return $"StringValue is {StringValue}";
    }

    private string _value;

    public void set(string value)
    {
        origin(value);
        fix(this, value);
    }

    public void origin(string value)
    {
        _value = value;
    }

    public static void fix(TestData data, string value)
    {
        Debug.Log(data.StringValue);
        Debug.Log(value);
    }
}