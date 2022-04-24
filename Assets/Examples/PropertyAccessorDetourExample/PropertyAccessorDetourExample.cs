using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using DataBinding;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// 属性访问器改道方案探究
/// </summary>
public class PropertyAccessorDetourExample : MonoBehaviour
{
    private TestData _data;

    private TestData _test8data;

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
        _test8data = new TestData();
        _test8data.StringValue = "test8data";

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
         * 尝试7 在DynamicMethod的方法中用IL写入临时变量,以此方式传递参数
         * 结果:可行,可以实现参数传递到newDynamicMethod的方法中
         * 问题:这个代码在自家电脑可以运行,但在公司电脑会崩溃,怀疑是.net版本问题
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;

            /*origin#1#
            var originMethod = propertyInfo.GetSetMethod();

            /*fix#1#
            Action<string> fixAction = (value) =>
            {
                var o = (object) this;
                var data = (TestData) o;

                Debug.Log($"type is {this.GetType()}, {data.StringValue}");
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

            // var fixmd = (Action<TestData, string>) fixDynamicMethod.CreateDelegate(typeof(Action<TestData, string>));

            /*new#1#
            var newDynamicMethod = new DynamicMethod($"new",
                typeof(void),
                new[] {typeof(TestData), typeof(string)},
                typeof(TestData));

            il = newDynamicMethod.GetILGenerator(256);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, originMethod, null);
            // 用DM的方式定义局部变量,可以实现值的传递
            // 关于在il中添加局部变量https://stackoverflow.com/questions/15278566/emit-local-variable-and-assign-a-value-to-it
            il.DeclareLocal(typeof(string));
            il.Emit(OpCodes.Ldstr, propertyName);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.EmitCall(OpCodes.Call, fixDynamicMethod, null);
            //
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Ldarg_1);
            // il.EmitCall(OpCodes.Call, fixDynamicMethod, null);
            il.Emit(OpCodes.Ret);

            var newmd = (Action<TestData, string>) newDynamicMethod.CreateDelegate(typeof(Action<TestData, string>));

            Memory.DetourMethod(originMethod, newDynamicMethod);
        }*/

        /*
         * 尝试8 基于尝试7将string换成实例
         * 结果:暂时不能使用局部实例引用,而是在方法内构造实例
         */
        /*var properties = typeof(TestData).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            var propertyName = propertyInfo.Name;

            /*origin#1#
            var originMethod = propertyInfo.GetSetMethod();

            /*fix#1#
            Action<TestData> fixAction = (data) =>
            {
                Debug.Log($"type is {this.GetType()}");
                Debug.Log($"data is {data.GetType()}");
                Debug.Log($"data StringValue is {data.StringValue}");
            };

            /*这一步的目的是将fixAction的所有权转到TestData#1#
            var fixDynamicMethod = new DynamicMethod($"fix",
                typeof(void),
                new[] {typeof(TestData), typeof(string)},
                typeof(TestData));

            var il = fixDynamicMethod.GetILGenerator(256);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, fixAction.Method, null);
            il.Emit(OpCodes.Ret);

            /*new#1#
            var newDynamicMethod = new DynamicMethod($"new",
                typeof(void),
                new[] {typeof(TestData), typeof(string)},
                typeof(TestData));

            il = newDynamicMethod.GetILGenerator(256);
            /*originMethod(value)#1#
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, originMethod, null);
            /*TestData property = new TestData();#1#
            var dl = il.DeclareLocal(typeof(TestData));
            il.Emit(OpCodes.Newobj, typeof(TestData).GetConstructors()[0]);
            /*fixDynamicMethod(property);#1#
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.EmitCall(OpCodes.Call, fixDynamicMethod, null);
            il.Emit(OpCodes.Ret);

            Memory.DetourMethod(originMethod, newDynamicMethod);
        }*/

        /*
         * 
         */
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
    private TestData _data;

    public void set(string value)
    {
        // var propertyName = "StringValue";

        origin(value);
        fix(this, value);
        var data = new TestData();
        set_property(data);
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

    public void set_property(TestData d)
    {
    }
}

public class DataExample
{
    private string _stringValue;

    public string StringValue
    {
        get { return _stringValue; }
        set
        {
            _stringValue = value;
            StaticSetEventMethod(this, "StringValue", value);
            LocalSetEventMethod(value);
        }
    }

    /*
     * 注入方法1 注入静态方法
     * 优点:
     *      1. 只需要注入一个固定的静态方法即可
     *      2. 静态方法可以缓存,减少了创建动态方法的开销
     * 缺点:
     *      1. 在方法的开销上,有3次字典查询,字典查询固定消耗较大
     */
    private static void StaticSetEventMethod(object obj, string propertyName, string value)
    {
        // 过程:
        // 1. 通过this查静态全局绑定集合,获取binding实例
        // 2. 通过propertyName找到对应PropertyEvent
        // 3. 调用PropertyEvent的事件

        // 需求:
        // 1. 需要知道obj,即调用方的this
        // 2. 需要知道propertyName,在动态方法的时候要把参数传进去
    }

    /*
     * 注入方法2 注入实例方法
     * 优点:
     *      1. 事件方法可以快速调用,避免了每次调用的字典查询(将字典查询的开销放到了生成动态方法的时候)
     * 缺点:
     *      1. 在动态生成方法的时候,每个属性都需要生成各自的方法,在初始化的时候成本较高
     * 难点:
     *      1. 如何给每个方法的临时变量赋值需要的propertyEvent引用
     *      2. 需要给数据类动态生成局部变量
     */
    private void LocalSetEventMethod(string value)
    {
        PropertyEvent propertyEvent = null;
        // 过程:
        // 1. 直接调用propertyEvent的set事件

        // 需求:
        // 1. 需要知道属性对应的propertyEvent
    }
}

public class BindingSimulate
{
    public Dictionary<string, PropertyEvent> _PropertyEvents;
}