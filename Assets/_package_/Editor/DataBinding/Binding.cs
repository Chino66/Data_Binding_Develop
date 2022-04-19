using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace DataBinding
{
    public class Binding
    {
        private object bindingObject;

        public object BindingObject => bindingObject;

        private Dictionary<string /*property name*/, PropertyEvent> _propertyEvents;

        private Dictionary<string /*property name*/, PropertyInfo> _propertyInfos;

        private Dictionary<string /*property name*/, Delegate> _delegates;

        public Binding(object bindingObject)
        {
            this.bindingObject = bindingObject;

            /*
             * todo 对绑定类型的属性进行缓存
             * 原因:https://lotsacode.wordpress.com/2010/04/13/reflection-type-getproperties-and-performance/
             */
            _propertyInfos = new Dictionary<string, PropertyInfo>();
            var properties = bindingObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                _propertyInfos.Add(property.Name, property);
            }

            _propertyEvents = new Dictionary<string, PropertyEvent>(properties.Length);
            _delegates = new Dictionary<string, Delegate>(properties.Length);

            _binding();
        }

        public PropertyInfo GetPropertyInfoByName(string propertyName)
        {
            _propertyInfos.TryGetValue(propertyName, out var propertyInfo);
            return propertyInfo;
        }

        public T GetPropertyValue<T>(string propertyName)
        {
            if (_propertyInfos.TryGetValue(propertyName, out var propertyInfo))
            {
                /*
                 * todo 反射获取性能优化,尝试使用委托
                 * https://lotsacode.wordpress.com/2010/04/12/getting-data-through-reflection-getvalue/
                 * https://www.codeproject.com/Articles/14560/Fast-Dynamic-Property-Field-Accessors
                 */
                return (T) propertyInfo.GetValue(bindingObject);
            }

            return default;
        }

        public void SetPropertyValue<T>(string propertyName, T value)
        {
            if (_delegates.TryGetValue(propertyName, out var dlg) == false)
            {
                var method = _propertyInfos[propertyName].GetSetMethod();
                dlg = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), bindingObject, method);
                _delegates.Add(propertyName, dlg);
            }

            ((Action<T>) dlg)(value);

            /*if (_propertyInfos.TryGetValue(propertyName, out var propertyInfo))
            {
                propertyInfo.SetValue(bindingObject, value);
            }*/
        }

        public void Dispose()
        {
            foreach (var pair in _propertyEvents)
            {
                pair.Value.Dispose();
            }

            _propertyEvents = null;
            _propertyInfos = null;
            var type = bindingObject.GetType();
            BindingCollection.BindingTypeRecord[type].Remove(bindingObject);
            bindingObject = null;
        }

        private void _binding()
        {
            var type = bindingObject.GetType();

            if (!BindingCollection.HasBinding(type))
            {
                BindingCollection.RegisterBinding(type);
            }

            BindingCollection.BindingTypeRecord[type].Add(bindingObject, this);
        }

        public void RegisterPreGetEvent<T>(string propertyName, Action<T> action)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                propertyEvent = new PropertyEvent<T>();
                _propertyEvents.Add(propertyName, propertyEvent);
            }

            ((PropertyEvent<T>) propertyEvent).PreGetEvent += action;
        }

        internal void OnPreGet<T>(string propertyName, T value)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                return;
            }

            ((PropertyEvent<T>) propertyEvent).PreGetEvent?.Invoke(value);
        }

        public void RegisterPostGetEvent<T>(string propertyName, Action<T> action)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                propertyEvent = new PropertyEvent<T>();
                _propertyEvents.Add(propertyName, propertyEvent);
            }

            ((PropertyEvent<T>) propertyEvent).PostGetEvent += action;
        }

        internal void OnPostGet<T>(string propertyName, T value)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                return;
            }

            ((PropertyEvent<T>) propertyEvent).PostGetEvent?.Invoke(value);
        }

        public void RegisterPreSetEvent<T>(string propertyName, Action<T> action)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                propertyEvent = new PropertyEvent<T>();
                _propertyEvents.Add(propertyName, propertyEvent);
            }

            ((PropertyEvent<T>) propertyEvent).PreSetEvent += action;
        }

        internal void OnPreSet<T>(string propertyName, T value)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                return;
            }

            ((PropertyEvent<T>) propertyEvent).PreSetEvent?.Invoke(value);
        }

        public void RegisterPostSetEvent<T>(string propertyName, Action<T> action)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                propertyEvent = new PropertyEvent<T>();
                _propertyEvents.Add(propertyName, propertyEvent);
            }

            ((PropertyEvent<T>) propertyEvent).PostSetEvent += action;
        }

        internal void OnPostSet<T>(string propertyName, T value)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                return;
            }

            ((PropertyEvent<T>) propertyEvent).PostSetEvent?.Invoke(value);
        }
    }
}