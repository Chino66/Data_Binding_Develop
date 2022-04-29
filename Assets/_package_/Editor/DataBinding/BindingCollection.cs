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
        /*todo 双层字典和一层字典的查询效率比较*/
        public static Dictionary<Type, Dictionary<object, Binding>> BindingTypeRecord;

        private static Dictionary<Type, BindingTypeCache> _bindingTypeCaches;

        static BindingCollection()
        {
            BindingTypeRecord = new Dictionary<Type, Dictionary<object, Binding>>();
            _bindingTypeCaches = new Dictionary<Type, BindingTypeCache>();
        }

        public static bool HasBinding(Type type)
        {
            return _bindingTypeCaches.ContainsKey(type);
        }

        public static bool RegisterBinding(Type type)
        {
            if (HasBinding(type))
            {
                return false;
            }

            var setValueDetourMethod =
                typeof(BindingCollection).GetMethod(nameof(SetValueDetour),
                    BindingFlags.Static | BindingFlags.NonPublic);

            var postSetValueMethod = typeof(BindingCollection).GetMethod(nameof(PostSetValue),
                BindingFlags.Static | BindingFlags.NonPublic);

            var bindingType = new BindingTypeCache(type);
            _bindingTypeCaches.Add(type, bindingType);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (var index = 0; index < properties.Length; index++)
            {
                var property = properties[index];
                var setMethod = property.SetMethod;

                var postSetMethod = postSetValueMethod.MakeGenericMethod(property.PropertyType);

                var detourMethod = setValueDetourMethod.MakeGenericMethod(property.PropertyType);

                var parameterTypes = new Type[detourMethod.GetParameters().Length];

                for (var i = 0; i < detourMethod.GetParameters().Length; i++)
                {
                    parameterTypes[i] = detourMethod.GetParameters()[i].ParameterType;
                }

                var dynamicMethod = new DynamicMethod("",
                    typeof(void),
                    parameterTypes,
                    type);

                var il = dynamicMethod.GetILGenerator();

                /*orig(self, value)*/
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, detourMethod);
                /*int x = index*/
                il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldc_I4, index);
                il.Emit(OpCodes.Stloc_0);
                /*OnSetEvent(...)*/
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, postSetMethod);

                il.Emit(OpCodes.Ret);

                IDetour detour = new Hook(setMethod, dynamicMethod);
            }

            BindingTypeRecord.Add(type, new Dictionary<object, Binding>());

            return true;
        }

        internal static void PostSetValue<T>(object instance, T value, int index)
        {
            var binding = _getBinding(instance);
            binding?.OnPostSet(index, value);
            // Debug.Log("PostSetValue");
        }

        internal static void SetValueDetour<T>(Action<object, T> orig, object self, T value)
        {
            // Debug.Log($"on set value ,value is {value}, type is {typeof(T)}, orig name is {orig.Method.Name}");
            orig(self, value);
        }


        private static Binding _getBinding(object instance)
        {
            var type = instance.GetType();

            if (BindingTypeRecord.TryGetValue(type, out var bindings))
            {
                return bindings.TryGetValue(instance, out var binding) ? binding : null;
            }

            return null;
        }
    }
}