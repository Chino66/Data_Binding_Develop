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

        // private Dictionary<string /*property name*/, PropertyEvent> _propertyEvents;
        //
        // private Dictionary<string /*property name*/, PropertyInfo> _propertyInfos;
        //
        // private Dictionary<string /*property name*/, Delegate> _delegates;

        /*
         * todo 共用
         */
        private PropertyInfo[] _propertyInfos;
        private PropertyEvent[] _propertyEvents;

        private Delegate[] _delegates;

        /*
         * todo 共用
         */
        private Dictionary<string, int> _propertyName2IndexMap;

        public Binding(object bindingObject)
        {
            this.bindingObject = bindingObject;

            _propertyName2IndexMap = new Dictionary<string, int>();
            /*
             * todo 对绑定类型的属性进行缓存
             * 原因:https://lotsacode.wordpress.com/2010/04/13/reflection-type-getproperties-and-performance/
             */
            var properties = bindingObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var lenght = properties.Length;
            _propertyInfos = properties;
            _propertyEvents = new PropertyEvent[lenght];
            _delegates = new Delegate[lenght];

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                // _propertyEvents[i] = new PropertyEvent();
                _propertyName2IndexMap.Add(property.Name, i);
            }

            _binding();
        }

        public int GetIndexByPropertyName(string propertyName)
        {
            _propertyName2IndexMap.TryGetValue(propertyName, out var index);
            return index;
        }

        public PropertyInfo GetPropertyInfoByName(string propertyName)
        {
            _propertyName2IndexMap.TryGetValue(propertyName, out var index);
            return GetPropertyInfoByIndex(index);
        }

        public PropertyInfo GetPropertyInfoByIndex(int index)
        {
            if (index < 0 || index > _propertyInfos.Length)
            {
                return null;
            }

            return _propertyInfos[index];
        }

        public PropertyEvent GetPropertyEventByName(string propertyName)
        {
            _propertyName2IndexMap.TryGetValue(propertyName, out var index);
            return GetPropertyEventByIndex(index);
        }

        public PropertyEvent GetPropertyEventByIndex(int index)
        {
            if (index < 0 || index >= _propertyEvents.Length)
            {
                return null;
            }

            return _propertyEvents[index];
        }

        /*public T GetPropertyValue<T>(string propertyName)
        {
            if (_propertyInfos.TryGetValue(propertyName, out var propertyInfo))
            {
                /*
                 * todo 反射获取性能优化,尝试使用委托
                 * https://lotsacode.wordpress.com/2010/04/12/getting-data-through-reflection-getvalue/
                 * https://www.codeproject.com/Articles/14560/Fast-Dynamic-Property-Field-Accessors
                 #1#
                return (T) propertyInfo.GetValue(bindingObject);
            }

            return default;
        }*/

        public T GetPropertyValue<T>(int index)
        {
            var propertyInfo = GetPropertyInfoByIndex(index);
            if (propertyInfo != null)
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

        /*public void SetPropertyValue<T>(string propertyName, T value)
        {
            if (_delegates.TryGetValue(propertyName, out var dlg) == false)
            {
                var method = _propertyInfos[propertyName].GetSetMethod();
                dlg = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), bindingObject, method);
                _delegates.Add(propertyName, dlg);
            }

            ((Action<T>) dlg)(value);
        }*/

        public void SetPropertyValue<T>(string propertyName, T value)
        {
            var index = GetIndexByPropertyName(propertyName);
            SetPropertyValue(index, value);
        }

        public void SetPropertyValue<T>(int index, T value)
        {
            // if (_delegates.TryGetValue(propertyName, out var dlg) == false)
            // {
            //     var method = _propertyInfos[propertyName].GetSetMethod();
            //     dlg = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), bindingObject, method);
            //     _delegates.Add(propertyName, dlg);
            // }
            //
            // ((Action<T>) dlg)(value);
            if (index < 0 || index >= _delegates.Length)
            {
                return;
            }

            if (_delegates[index] == null)
            {
                var method = _propertyInfos[index].GetSetMethod();
                var dlg = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), bindingObject, method);
                _delegates[index] = dlg;
            }

            ((Action<T>) _delegates[index])(value);
        }

        public void Dispose()
        {
            foreach (var pair in _propertyEvents)
            {
                pair.Dispose();
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

        /*public void RegisterPreGetEvent<T>(string propertyName, Action<T> action)
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
        }*/

        /*public void RegisterPostSetEvent<T>(string propertyName, Action<T> action)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                propertyEvent = new PropertyEvent<T>();
                _propertyEvents.Add(propertyName, propertyEvent);
            }

            ((PropertyEvent<T>) propertyEvent).PostSetEvent += action;
        }*/

        /*public void RegisterPostSetEvent<T>(string propertyName, Action<T> action)
        {
            var propertyEvent = GetPropertyEventByName(propertyName);
            if (propertyEvent != null)
            {
                ((PropertyEvent<T>) propertyEvent).PostSetEvent += action;
            }
        }*/

        public void RegisterPostSetEvent<T>(int index, Action<T> action)
        {
            var propertyEvent = GetPropertyEventByIndex(index);
            if (propertyEvent == null)
            {
                propertyEvent = new PropertyEvent<T>();
                _propertyEvents[index] = propertyEvent;
            }

            ((PropertyEvent<T>) propertyEvent).PostSetEvent += action;
        }

        internal void OnPostSet<T>(int index, T value)
        {
            var propertyEvent = GetPropertyEventByIndex(index);
            ((PropertyEvent<T>) propertyEvent)?.PostSetEvent?.Invoke(value);
        }

        /*internal void OnPostSet<T>(string propertyName, T value)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                return;
            }

            ((PropertyEvent<T>) propertyEvent).PostSetEvent?.Invoke(value);
        }*/
    }
}