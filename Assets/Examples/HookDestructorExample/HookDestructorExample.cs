using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace HDE
{
    public class HookDestructorExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Main();
        }

        private void Main()
        {
            var destructorMethod = typeof(HDEData).GetMethod("Finalize",
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            Debug.Log($"{destructorMethod.Name}");

            var finalizeMethod =
                this.GetType().GetMethod(nameof(Finalize), BindingFlags.Static | BindingFlags.Public);
            Debug.Log($"{finalizeMethod.Name}");


            var dynamicDestructor = new DynamicMethod("Finalize",
                typeof(void),
                new[] {typeof(HDEData)},
                typeof(HDEData));

            var il = dynamicDestructor.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "out put");
            il.Emit(OpCodes.Call, getLogMethodInfo());
            il.Emit(OpCodes.Ret);

            Debug.Log(dynamicDestructor.Name);

            // IDetour i = new Hook(destructorMethod, finalizeMethod);

            var data = new HDEData();
            data.StringValue = "66";

            /*
             * todo 如果直接获取"Finalize"方法,则获得的方法是object的"Finalize",此时如果改道方法,那么所有类都会被改道
             */
        }

        public static void Finalize(Action<object> orig, object self)
        {
            Debug.Log($"Finalize type is {self.GetType().Name}");
            orig(self);
        }

        public void f()
        {
            Debug.Log("......");
        }

        private MethodInfo getLogMethodInfo()
        {
            return typeof(Debug).GetMethod(nameof(Debug.Log), new[] {typeof(object)});
        }
    }


    public class HDEData
    {
        public string StringValue;

        // ~HDEData()
        // {
        // }
    }
}