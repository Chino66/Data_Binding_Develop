using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace DME
{
    public class DynamicMethodExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Main();
        }

        private void Main()
        {
            var data = new DMEData();
            data.StringValue = "66";

            Action<string> action = (value) => { Debug.Log($"this is {this.GetType().Name}, value is {value}"); };
            var testMethod = this.GetType().GetMethod(nameof(TestMethod), BindingFlags.Instance | BindingFlags.Public);
            var staticTestMethod = this.GetType()
                .GetMethod(nameof(StaticTestMethod), BindingFlags.Static | BindingFlags.Public);

            /*测试1 类方法或lambda方法,填入不同实例的运行结果*/
            /*将类中的方法转为委托,填入自身实例,可以运行*/
            /*var testMethodDelegate = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), this, testMethod);
            testMethodDelegate("testMethodDelegate");*/
            /*填入data实例,不能运行,如果填入this则可以运行*/
            /*var actionDelegate = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), data, action.Method);
            actionDelegate("actionDelegate");*/
            /*
             * 原因是action方法的所有者不是data实例
             * Delegate.CreateDelegate默认将创建的委托的所有者是当前类实例
             */


            /*
             * 测试2 DynamicMethod只支持静态方法吗
             * DynamicMethod可以静态方法也可以实例方法
             * https://docs.microsoft.com/zh-cn/dotnet/api/system.reflection.emit.dynamicmethod.createdelegate?view=net-6.0#system-reflection-emit-dynamicmethod-createdelegate(system-type-system-object)
             */
            var dm = new DynamicMethod("",
                typeof(void),
                /*这是核心,第一个参数必须是调用的实例类,它和action的参数是不同的*/
                new[] {typeof(DMEData), typeof(string)},
                typeof(DMEData));

            var il = dm.GetILGenerator(256);
            il.Emit(OpCodes.Ldarg_0); // 载入参数 data (DMEData)
            il.Emit(OpCodes.Ldarg_1); // 载入参数 value (string)
            il.EmitCall(OpCodes.Call, action.Method, null);
            il.Emit(OpCodes.Ret);

            /*实例方式调用*/
            var instanceMethod = (Action<string>) dm.CreateDelegate(typeof(Action<string>), data);
            instanceMethod("instanceMethod");

            /*静态方法调用*/
            var staticMethod = (Action<DMEData, string>) dm.CreateDelegate(typeof(Action<DMEData, string>));
            staticMethod(data, "staticMethod");

            /*
             * 结论:
             *  DynamicMethod和Delegate.CreateDelegate的区别在于DynamicMethod可以指定方法的所有者
             *  而Delegate.CreateDelegate创建的委托的方法所有者是当前类
             * 
             */
        }

        public void TestMethod(string value)
        {
            Debug.Log($"this is TestMethod, value is {value}");
        }

        public static void StaticTestMethod(string value)
        {
            Debug.Log($"this is StaticTestMethod, value is {value}");
        }
    }

    public class DMEData
    {
        public string StringValue { get; set; }
    }
}