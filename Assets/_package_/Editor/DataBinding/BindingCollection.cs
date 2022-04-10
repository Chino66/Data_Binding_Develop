using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DataBinding
{
    public static class BindingCollection
    {
        public static Dictionary<Type, Dictionary<object, Binding>> BindingTypeRecord;

        static BindingCollection()
        {
            BindingTypeRecord = new Dictionary<Type, Dictionary<object, Binding>>();
        }

        public static bool HasBinding(Type type)
        {
            return BindingTypeRecord.ContainsKey(type);
        }

        public static bool RegisterBinding(Type type)
        {
            if (HasBinding(type))
            {
                return false;
            }

            // todo binding Type
            var harmony = new Harmony("com.chino.data.binding.patch");

            var per = typeof(BindingCollection).GetMethod("PreSetValue");
            var post = typeof(BindingCollection).GetMethod("PostSetValue");


            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var set = property.SetMethod;
                harmony.Patch(set, new HarmonyMethod(per), new HarmonyMethod(post));
            }

            BindingTypeRecord.Add(type, new Dictionary<object, Binding>());

            return true;
        }

        public static void PreSetValue(object __instance, MethodInfo __originalMethod, object value)
        {
            // var setMethodName = __originalMethod.Name;
            //
            // // todo use spin no gc
            // var propertyName = setMethodName.Substring(4, setMethodName.Length - 4);
            //
            // var type = __instance.GetType();
            // if (BindingTypeRecord.TryGetValue(type, out var bindings))
            // {
            //     if (bindings.TryGetValue(__instance, out var binding))
            //     {
            //         binding.OnSet(propertyName, value);
            //     }
            // }
        }

        public static void PostSetValue(object __instance, MethodInfo __originalMethod, object value)
        {
            // Debug.Log($"post set value {__originalMethod.Name}");
            var setMethodName = __originalMethod.Name;

            // todo use spin no gc
            var propertyName = setMethodName.Substring(4, setMethodName.Length - 4);

            var type = __instance.GetType();
            if (BindingTypeRecord.TryGetValue(type, out var bindings))
            {
                if (bindings.TryGetValue(__instance, out var binding))
                {
                    binding.OnSet(propertyName, value);
                }
            }
        }
    }
}