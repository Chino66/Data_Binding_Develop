using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace DataBinding
{
    public static class BindingCollection
    {
        private static readonly Dictionary<Type, BindingTypeCache> BindingTypeCaches;

        private static readonly MethodInfo SetValueMethod;
        private static readonly MethodInfo PostSetValueMethod;

        private static readonly Dictionary<Type, MethodInfo> SetValueMethodGenerateCahce;
        private static readonly Dictionary<Type, MethodInfo> PostSetValueMethodGenerateCahce;

        static BindingCollection()
        {
            BindingTypeCaches = new Dictionary<Type, BindingTypeCache>();

            SetValueMethodGenerateCahce = new Dictionary<Type, MethodInfo>();
            PostSetValueMethodGenerateCahce = new Dictionary<Type, MethodInfo>();
            var type = typeof(BindingCollection);
            SetValueMethod = type.GetMethod(nameof(SetValue),
                BindingFlags.Static | BindingFlags.NonPublic);
            PostSetValueMethod = type.GetMethod(nameof(PostSetValue),
                BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static BindingTypeCache GetBindingTypeCache(object instance)
        {
            var type = instance.GetType();
            RegisterBinding(type);
            return BindingTypeCaches[type];
        }

        public static bool HasBinding(Type type)
        {
            return BindingTypeCaches.ContainsKey(type);
        }

        public static bool RegisterBinding(Type type)
        {
            if (HasBinding(type))
            {
                return false;
            }

            var bindingType = new BindingTypeCache(type);
            BindingTypeCaches.Add(type, bindingType);

            var properties = bindingType.PropertyInfos;
            for (var index = 0; index < properties.Length; index++)
            {
                var property = properties[index];

                var postSetValueMethod = GetPostSetValueMethodInfo(property.PropertyType);
                var setValueMethod = GetSetValueMethodInfo(property.PropertyType);

                var parameterTypes = new Type[setValueMethod.GetParameters().Length];

                for (var i = 0; i < setValueMethod.GetParameters().Length; i++)
                {
                    parameterTypes[i] = setValueMethod.GetParameters()[i].ParameterType;
                }

                var dynamicMethod = new DynamicMethod("",
                    typeof(void),
                    parameterTypes,
                    type);

                var il = dynamicMethod.GetILGenerator();

                /*SetValue(...)*/
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, setValueMethod);
                /*int x = index*/
                il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldc_I4, index);
                il.Emit(OpCodes.Stloc_0);
                /*PostSetValue(...)*/
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, postSetValueMethod);

                il.Emit(OpCodes.Ret);

                var originSetMethod = property.SetMethod;
                /*todo hold detour*/
                IDetour detour = new Hook(originSetMethod, dynamicMethod);
            }

            return true;
        }

        private static MethodInfo GetSetValueMethodInfo(Type type)
        {
            if (SetValueMethodGenerateCahce.TryGetValue(type, out var methodInfo) == false)
            {
                methodInfo = SetValueMethod.MakeGenericMethod(type);
                SetValueMethodGenerateCahce.Add(type, methodInfo);
            }

            return methodInfo;
        }

        private static MethodInfo GetPostSetValueMethodInfo(Type type)
        {
            if (PostSetValueMethodGenerateCahce.TryGetValue(type, out var methodInfo) == false)
            {
                methodInfo = PostSetValueMethod.MakeGenericMethod(type);
                PostSetValueMethodGenerateCahce.Add(type, methodInfo);
            }

            return methodInfo;
        }

        /*
         * 调用"SetValue"方法执行原方法orig,而不是在动态方法中直接调用orig
         * 原因是这样的方式在性能上更好
         */
        internal static void SetValue<T>(Action<object, T> orig, object self, T value)
        {
            orig(self, value);
        }

        internal static void PostSetValue<T>(object instance, T value, int index)
        {
            if (instance is IBindable bindable)
            {
                bindable.Binding?.OnPostSet(index, value);
            }
        }
    }
}