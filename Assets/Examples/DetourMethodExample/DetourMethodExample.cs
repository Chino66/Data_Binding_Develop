using System;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DME
{
    /// <summary>
    /// 改道方法示例
    /// 将方法指针转向目标方法实现方法的改道
    /// </summary>
    public class DetourMethodExample : MonoBehaviour
    {
        private ExternalClass _externalClass = new ExternalClass();

        private delegate void action();

        private void OnGUI()
        {
            if (GUILayout.Button("detour Method"))
            {
                Detour();
            }

            if (GUILayout.Button($"invoke {nameof(LocalMethodA)}"))
            {
                LocalMethodA();
            }

            if (GUILayout.Button($"invoke {nameof(LocalMethodB)}"))
            {
                LocalMethodB();
            }

            if (GUILayout.Button($"invoke {nameof(StaticMethodA)}"))
            {
                StaticMethodA();
            }

            if (GUILayout.Button($"invoke {nameof(ExternalClass.LocalMethodA_ExternalClass)}"))
            {
                _externalClass.LocalMethodA_ExternalClass();
            }
        }

        private void Detour()
        {
            /*
             * 改道方法:
             *  Memory.DetourMethod(ma, mb);
             * 等价于:
             *  Exception exception;
             *  long methodAStart = Memory.GetMethodStart(ma, out exception);
             *  long methodBStart = Memory.GetMethodStart(mb, out exception);
             *  Memory.WriteJump(methodAStart, methodBStart);
             */
            /*
             * 测试1 改道本地实例方法,将"LocalMethodA"指向"LocalMethodB"
             * 结果:成功,调用"LocalMethodA"实际执行"LocalMethodB"
             */
            /*var type = typeof(DetourMethodExample);
            var ma = type.GetMethod(nameof(LocalMethodA), BindingFlags.Instance | BindingFlags.Public);
            var mb = type.GetMethod(nameof(LocalMethodB), BindingFlags.Instance | BindingFlags.Public);
            Memory.DetourMethod(ma, mb);*/

            /*
             * 测试2 本地实例方法改道本地静态方法,将"LocalMethodA"指向"StaticMethodA"
             * 结果:成功,调用"LocalMethodA"实际执行"StaticMethodA"
             */
            /*var type = typeof(DetourMethodExample);
            var lma = type.GetMethod(nameof(LocalMethodA), BindingFlags.Instance | BindingFlags.Public);
            var sma = type.GetMethod(nameof(StaticMethodA), BindingFlags.Static | BindingFlags.Public);
            Memory.DetourMethod(lma, sma);*/

            /*
             * 测试3 本地实例方法改道外部实例方法,将"LocalMethodA"指向"LocalMethodA_ExternalClass"
             * 结果:成功,调用"LocalMethodA"执行"LocalMethodA_ExternalClass"
             * 此时"LocalMethodA_ExternalClass"的this指向"DetourMethodExample"
             * 拓展:能实现外部实例方法,也就能指向外部静态方法
             */
            /*var type = typeof(DetourMethodExample);
            var lma = type.GetMethod(nameof(LocalMethodA), BindingFlags.Instance | BindingFlags.Public);
            var type2 = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var elma = type2.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);
            Memory.DetourMethod(lma, elma);*/

            /*
             * 测试4 本地实例方法改道本地lambda函数,将"LocalMethodA"指向相同签名的lambda函数
             * 结果:成功,调用"LocalMethodA",执行action
             */
            /*Action action = () => { Debug.Log($"this is lambda"); };
            var type = typeof(DetourMethodExample);
            var lma = type.GetMethod(nameof(LocalMethodA), BindingFlags.Instance | BindingFlags.Public);
            Memory.DetourMethod(lma, action.Method);*/

            /*
             * 测试4-拓展 即便lambda签名不一样也能改道
             * 结果:可以改道,但value的值没有,报空了
             */
            /*Action<string> action2 = (value) =>
            {
                Debug.Log($"this is lambda");
                Debug.Log($"value is {value}");
            };
            var type = typeof(DetourMethodExample);
            var lma = type.GetMethod(nameof(LocalMethodA), BindingFlags.Instance | BindingFlags.Public);
            Memory.DetourMethod(lma, action2.Method);*/

            /*
             * 测试5 外部实例方法改道本地实例方法,将"LocalMethodA_ExternalClass"指向"LocalMethodA"
             * 结果:成功
             */
            /*var type = typeof(DetourMethodExample);
            var lma = type.GetMethod(nameof(LocalMethodA), BindingFlags.Instance | BindingFlags.Public);
            var type2 = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var elma = type2.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);
            Memory.DetourMethod(elma, lma);*/

            /*
             * 测试6 外部实例方法改道本地lambda函数,将"LocalMethodA_ExternalClass"指向相同签名的lambda函数
             * 结果:可行,调用"LocalMethodA_ExternalClass"执行action
             * 此时action的this指向"ExternalClass"
             */
            /*Action action = () =>
            {
                Debug.Log($"this is lambda, type is {this.GetType()}");
            };
            var type = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var lma = type.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);
            Memory.DetourMethod(lma, action.Method);*/


            /*
             * 测试7 外部实例方法改道DynamicMethod创建方法内部调用的本地lambda函数
             *      将"LocalMethodA_ExternalClass"指向DynamicMethod
             * 结果:成功,调用"LocalMethodA_ExternalClass"执行DynamicMethod
             *      DynamicMethod的参数类型第一个必须是调用的类型才能调用实例方法
             */
            /*Action action = () => { Debug.Log($"this is lambda, type is {this.GetType()}"); };

            var dynamicMethod = new DynamicMethod("",
                typeof(void),
                new[] {typeof(ExternalClass)},
                typeof(ExternalClass));

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // 载入this才能调用实例方法
            il.Emit(OpCodes.Call, action.Method);
            il.Emit(OpCodes.Ret);
            var type = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var elma = type.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);
            var ret = Memory.DetourMethod(elma, dynamicMethod);

            if (ret != null)
            {
                Debug.LogError(ret);
            }*/

            /*
             * 测试7 进阶1 DynamicMethod创建方法定义临时变量,action接收变量并输出
             * 结果:成功,调用"LocalMethodA_ExternalClass"执行DynamicMethod,并输出定义的临时变量
             */
            /*Action<int> action = (value) =>
            {
                Debug.Log($"this is lambda, type is {this.GetType()}");
                Debug.Log($"value is {value}");
            };

            var dynamicMethod = new DynamicMethod("",
                typeof(void),
                new[] {typeof(ExternalClass)},
                typeof(ExternalClass));

            var il = dynamicMethod.GetILGenerator();
            il.DeclareLocal(typeof(int));
            /*int x = 100#1#
            il.Emit(OpCodes.Ldc_I4, 100);
            il.Emit(OpCodes.Stloc_0);
            /*action(x)#1#
            il.Emit(OpCodes.Ldarg_0); // 载入this才能调用实例方法
            il.Emit(OpCodes.Ldloc_0); // 载入this才能调用实例方法
            il.Emit(OpCodes.Call, action.Method);
            il.Emit(OpCodes.Ret);
            var type = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var elma = type.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);
            var ret = Memory.DetourMethod(elma, dynamicMethod);

            if (ret != null)
            {
                Debug.LogError(ret);
            }*/

            /*
             * 测试7 进阶2 "LocalMethodA_ExternalClass"改道DynamicMethod,但仍然被DynamicMethod调用
             * 结果:失败
             * 分析:猜测原因是"LocalMethodA_ExternalClass"改道后,实际的MethodInfo信息被修改
             *      导致DynamicMethod调用"LocalMethodA_ExternalClass"实际是在调用自身,造成死循环
             * 改进:尝试寻找一个方式保留"LocalMethodA_ExternalClass"
             *      委托?
             */
            /*var type = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var elma = type.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);

            Action<int> action = (value) =>
            {
                Debug.Log($"this is lambda, type is {this.GetType()}");
                Debug.Log($"value is {value}");
            };

            var dynamicMethod = new DynamicMethod("",
                typeof(void),
                new[] {typeof(ExternalClass)},
                typeof(ExternalClass));

            var il = dynamicMethod.GetILGenerator();
            il.DeclareLocal(typeof(int));
            /*int x = 100#1#
            il.Emit(OpCodes.Ldc_I4, 100);
            il.Emit(OpCodes.Stloc_0);
            /*action(x)#1#
            il.Emit(OpCodes.Ldarg_0); // 载入this才能调用实例方法
            il.Emit(OpCodes.Ldloc_0); 
            il.Emit(OpCodes.Call, action.Method);
            /*LocalMethodA_ExternalClass()#1#
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, elma);
            il.Emit(OpCodes.Ret);

            var ret = Memory.DetourMethod(elma, dynamicMethod);

            if (ret != null)
            {
                Debug.LogError(ret);
            }*/

            /*
             * 测试7 进阶3 解决上面问题:
             *      1. 创建一个中间方法X,将X改道"LocalMethodA_ExternalClass",即调用X,执行""LocalMethodA_ExternalClass"指向"
             *      2. DynamicMethod调用X
             *      3. "LocalMethodA_ExternalClass"改道DynamicMethod
             */
            // Action proxy = () =>
            // {
            //     Debug.Log($"proxy");
            // };
            var type = typeof(ExternalClass);
            var elmaName = nameof(ExternalClass.LocalMethodA_ExternalClass);
            var elma = type.GetMethod(elmaName, BindingFlags.Instance | BindingFlags.Public);

            var d = (action) Delegate.CreateDelegate(typeof(action), _externalClass, elma);
            Debug.Log($"is same {d.Method == elma}");
            // var ret = Memory.DetourMethod(proxy.Method, elma);
            // if (ret != null)
            // {
            //     Debug.LogError(ret);
            // }

            // proxy.Invoke();

            Action<int> action = (value) =>
            {
                Debug.Log($"this is lambda, type is {this.GetType()}");
                Debug.Log($"value is {value}");
            };

            var dynamicMethod = new DynamicMethod("",
                typeof(void),
                new[] {typeof(ExternalClass), typeof(string)},
                typeof(ExternalClass));

            var il = dynamicMethod.GetILGenerator();
            il.DeclareLocal(typeof(int));
            /*int x = 100*/
            il.Emit(OpCodes.Ldc_I4, 100);
            il.Emit(OpCodes.Stloc_0);
            /*action(x)*/
            il.Emit(OpCodes.Ldarg_0); // 载入this才能调用实例方法
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, action.Method);
            /*LocalMethodA_ExternalClass()*/
            // il.Emit(OpCodes.Ldarg_0);
            // il.Emit(OpCodes.Call, d.Method);
            il.Emit(OpCodes.Ret);

            var ret = Memory.DetourMethod(elma, dynamicMethod);

            if (ret != null)
            {
                Debug.LogError(ret);
            }

            /**/
            Debug.Log("invoke Detour");

            /*var harmony = new Harmony("com.chino.data.binding.patch");
            harmony.Patch(set, new HarmonyMethod(preGenericMethod), new HarmonyMethod(postGenericMethod));*/
        }


        public void LocalMethodA()
        {
            Debug.Log($"this is {nameof(LocalMethodA)}, type is {this.GetType()}");
        }

        public void LocalMethodB()
        {
            Debug.Log($"this is {nameof(LocalMethodB)}, type is {this.GetType()}");
        }

        public static void StaticMethodA()
        {
            Debug.Log($"this is {nameof(StaticMethodA)}");
        }
    }

    public class ExternalClass
    {
        public void LocalMethodA_ExternalClass()
        {
            Debug.Log($"this is {nameof(LocalMethodA_ExternalClass)}, type is {this.GetType()}");
        }

        public void LocalMethodB_ExternalClass()
        {
            Debug.Log($"this is {nameof(LocalMethodB_ExternalClass)}, type is {this.GetType()}");
        }

        public static void StaticMethodA_ExternalClass()
        {
            Debug.Log($"this is {nameof(StaticMethodA_ExternalClass)}");
        }
    }

    public class ILExample
    {
        private string _value;

        public void set(string value)
        {
            _value = value;
            set_string(value);
        }


        public void method()
        {
            int x = 100;
            action(x);
        }

        public void action(int x)
        {
        }

        public void set_string(string value)
        {
        }
    }
}