using System;
using System.Linq;
using System.Reflection;
using DataBinding;
using UnityEngine;
using UnityEngine.UIElements;

namespace _package_.Editor
{
    public class UIElementBinding
    {
        private readonly Binding _binding;

        public UIElementBinding(Binding binding)
        {
            this._binding = binding;
        }


        public void Bind<T>(T element) where T : BindableElement
        {
            /*1. 组件是否有BindableElement,有则获取绑定属性名称*/
            if (!(element is BindableElement bindable))
            {
                Debug.LogError("bindable is null");
                return;
            }

            var propertyName = bindable.bindingPath;

            /*2. 组件是否实现INotifyValueChanged<>接口,有则获取组件泛型类型*/
            var type = element.GetType();
            var interfaces = type.GetInterfaces();
            var genericType =
                (from itf in interfaces
                    where itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(INotifyValueChanged<>)
                    select itf.GenericTypeArguments[0]).FirstOrDefault();

            if (genericType == null)
            {
                Debug.LogError("genericType is null");
                return;
            }

            Debug.Log($"genericType {genericType.Name}");

            /*3. 数据类型*/
            var propertyInfo = _binding.GetPropertyInfoByName(propertyName);
            if (propertyInfo == null)
            {
                Debug.LogError("propertyInfo is null");
                return;
            }

            var propertyType = propertyInfo.PropertyType;

            Debug.Log($"propertyType {propertyType.Name}");

            var index = _binding.GetIndexByPropertyName(propertyName);
            if (propertyType == genericType)
            {
                var method = this.GetType()
                    .GetMethod(nameof(_bindSameType), BindingFlags.Instance | BindingFlags.NonPublic);
                method = method.MakeGenericMethod(propertyType);
                method.Invoke(this, new object[] {_binding, index, element});
            }
            else
            {
                var method = this.GetType()
                    .GetMethod(nameof(_bindDiffType), BindingFlags.Instance | BindingFlags.NonPublic);
                method = method.MakeGenericMethod(propertyType, genericType);
                method.Invoke(this, new object[] {_binding, index, element});
            }
        }

        private void _bindSameType<T>(Binding binding, int index, INotifyValueChanged<T> valueChanged)
        {
            /*数据绑定组件*/
            binding.RegisterPostSetEvent<T>(index, o => { valueChanged.value = o; });

            /*组件绑定数据*/
            valueChanged.RegisterValueChangedCallback(evt =>
            {
                var value = binding.GetPropertyValue<T>(index);
                if (value == null || value.Equals(evt.newValue) == false)
                {
                    binding.SetPropertyValue(index, evt.newValue);
                }
            });
        }

        private void _bindDiffType<T, T2>(Binding binding, int index, INotifyValueChanged<T2> valueChanged)
        {
            /*数据绑定组件*/
            binding.RegisterPostSetEvent<T>(index,
                o =>
                {
                    valueChanged.value = (T2) Convert.ChangeType(o, typeof(T2));
                });

            /*组件绑定数据*/
            valueChanged.RegisterValueChangedCallback(evt =>
            {
                var method = typeof(T).GetMethod("Parse", new[] {typeof(string)});
                T value = default;
                if (method != null)
                {
                    value = (T) method.Invoke(null, new object[] {evt.newValue});
                }
                else
                {
                    value = (T) Convert.ChangeType(evt.newValue, typeof(T));
                }

                if (value == null || binding.GetPropertyValue<T>(index).Equals(value) == false)
                {
                    binding.SetPropertyValue(index, value);
                }
            });
        }
    }
}