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

        public static Dictionary<string /*set method name*/, string /*property name*/> _propertyNameMap;

        static BindingCollection()
        {
            BindingTypeRecord = new Dictionary<Type, Dictionary<object, Binding>>();
            _propertyNameMap = new Dictionary<string, string>();
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

            var harmony = new Harmony("com.chino.data.binding.patch");

            /*todo 给属性的Get方法注入方法*/

            /*给属性的Set方法注入方法*/
            /*todo 使用dynamic method生成不同值类型,代替object类型,减少装箱拆箱的性能开销*/
            var per = typeof(BindingCollection).GetMethod(nameof(PreSetValue),
                BindingFlags.Static | BindingFlags.NonPublic);
            var post = typeof(BindingCollection).GetMethod(nameof(PostSetValue),
                BindingFlags.Static | BindingFlags.NonPublic);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var set = property.SetMethod;
                var preGenericMethod = per.MakeGenericMethod(property.PropertyType);
                var postGenericMethod = post.MakeGenericMethod(property.PropertyType);
                harmony.Patch(set, new HarmonyMethod(preGenericMethod), new HarmonyMethod(postGenericMethod));
                _propertyNameMap[set.Name] = property.Name;
            }

            BindingTypeRecord.Add(type, new Dictionary<object, Binding>());

            return true;
        }

        internal static void PreGetValue(object __instance, MethodInfo __originalMethod, object value)
        {
            var propertyName = _getPropertyName(__originalMethod);
            var binding = _getBinding(__instance);
            binding?.OnPreGet(propertyName, value);
        }

        internal static void PostGetValue(object __instance, MethodInfo __originalMethod, object value)
        {
            var propertyName = _getPropertyName(__originalMethod);
            var binding = _getBinding(__instance);
            binding?.OnPostGet(propertyName, value);
        }

        internal static void PreSetValue<T>(object __instance, MethodInfo __originalMethod, T value)
        {
            var propertyName = _getPropertyName(__originalMethod);
            var binding = _getBinding(__instance);
            binding?.OnPreSet(propertyName, value);

            /*
             * todo 使用dynamic method包装反射方法提升性能
             * https://stackoverflow.com/questions/10313979/methodinfo-invoke-performance-issue
             * 快速反射框架 https://github.com/buunguyen/fasterflect
             * https://www.codeproject.com/Articles/14593/A-General-Fast-Method-Invoker
             * https://www.automatetheplanet.com/optimize-csharp-reflection-using-delegates/
             * https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
             */
            /*var method = typeof(Binding).GetMethod(nameof(Binding.OnPreSet),
                BindingFlags.NonPublic | BindingFlags.Instance);
            method = method.MakeGenericMethod(value.GetType());
            method.Invoke(binding, new[] {propertyName, value});*/
        }

        internal static void PostSetValue<T>(object __instance, MethodInfo __originalMethod, T value)
        {
            var propertyName = _getPropertyName(__originalMethod);
            var binding = _getBinding(__instance);
            binding?.OnPostSet(propertyName, value);

            /*var method = typeof(Binding).GetMethod(nameof(Binding.OnPostSet),
                BindingFlags.NonPublic | BindingFlags.Instance);
            method = method.MakeGenericMethod(value.GetType());
            method.Invoke(binding, new[] {propertyName, value});*/
        }

        private static string _getPropertyName(MethodInfo __originalMethod)
        {
            var setMethodName = __originalMethod.Name;
            // var propertyName = setMethodName.Substring(4, setMethodName.Length - 4);
            var propertyName = _propertyNameMap[setMethodName];
            return propertyName;
        }


        private static Binding _getBinding(object __instance)
        {
            var type = __instance.GetType();

            if (BindingTypeRecord.TryGetValue(type, out var bindings))
            {
                return bindings.TryGetValue(__instance, out var binding) ? binding : null;
            }

            return null;
        }
    }
}