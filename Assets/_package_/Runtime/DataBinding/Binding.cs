using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataBinding.BindingExtensions;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace DataBinding
{
    public class Binding
    {
        private object _bindingObject;
        public object BindingObject => _bindingObject;
        private readonly PropertyInfo[] _propertyInfos;
        private readonly PropertyEvent[] _propertyEvents;
        private readonly Delegate[] _setDelegates;
        private readonly Delegate[] _getDelegates;
        private readonly BindingTypeCache _bindingType;

        private Dictionary<string /*binding member name*/, Binding> _bindings;

        internal Binding(object bindingObject)
        {
            _bindingObject = bindingObject;
            _bindingType = BindingCollection.GetBindingTypeCache(_bindingObject);

            _propertyInfos = _bindingType.PropertyInfos;
            var lenght = _propertyInfos.Length;
            _propertyEvents = new PropertyEvent[lenght];
            _setDelegates = new Delegate[lenght];
            _getDelegates = new Delegate[lenght];

            BindingCollection.MakeBinding(_bindingObject, this);
        }

        /*
         * todo expression tree
         */
        public Binding FindBinding(string propertyName, bool constructIfNull = false)
        {
            _bindings ??= new Dictionary<string, Binding>();

            _bindings.TryGetValue(propertyName, out var binding);
            if (binding != null)
            {
                return binding;
            }

            var ret = _bindingType.GetIndexByPropertyName(propertyName, out var index);
            if (ret == false)
            {
                Debug.LogError($"type {_bindingType.Type.Name} has not {propertyName} property");
                return null;
            }

            var propertyInfo = _bindingType.GetPropertyInfoByIndex(index);
            /*
             * todo delegate
             */
            var obj = propertyInfo.GetValue(_bindingObject);
            if (obj == null)
            {
                if (constructIfNull == false)
                {
                    Debug.LogError($"{propertyName} instance is null, please construct it");
                    return null;
                }

                obj = Activator.CreateInstance(propertyInfo.PropertyType);
                /*
                 * todo delegate
                 */
                propertyInfo.SetValue(_bindingObject, obj);
            }

            binding = obj.GetBinding();
            /*
             * todo binding之间的事件传递用更好的方式实现
             */
            binding.RegisterPropertyChangeCallback(PropertyChangeCallback);
            _bindings.Add(propertyInfo.Name, binding);

            return binding;
        }

        public int GetIndexByPropertyName(string propertyName)
        {
            _bindingType.GetIndexByPropertyName(propertyName, out var index);
            return index;
        }

        public PropertyInfo GetPropertyInfoByName(string propertyName)
        {
            return _bindingType.GetPropertyInfoByName(propertyName);
        }

        public PropertyInfo GetPropertyInfoByIndex(int index)
        {
            return _bindingType.GetPropertyInfoByIndex(index);
        }

        public PropertyEvent GetPropertyEventByName(string propertyName)
        {
            _bindingType.GetIndexByPropertyName(propertyName, out var index);
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

        public T GetPropertyValue<T>(string propertyName)
        {
            _bindingType.GetIndexByPropertyName(propertyName, out var index);
            return GetPropertyValue<T>(index);
        }

        public T GetPropertyValue<T>(int index)
        {
            if (index < 0 || index >= _getDelegates.Length)
            {
                return default;
            }

            if (_getDelegates[index] == null)
            {
                var method = _propertyInfos[index].GetGetMethod();
                var dlg = (Func<T>) Delegate.CreateDelegate(typeof(Func<T>), _bindingObject, method);
                _getDelegates[index] = dlg;
            }

            return ((Func<T>) _getDelegates[index])();
        }


        public void SetPropertyValue<T>(string propertyName, T value)
        {
            _bindingType.GetIndexByPropertyName(propertyName, out var index);
            SetPropertyValue(index, value);
        }

        public void SetPropertyValue<T>(int index, T value)
        {
            if (index < 0 || index >= _setDelegates.Length)
            {
                return;
            }

            if (_setDelegates[index] == null)
            {
                var method = _propertyInfos[index].GetSetMethod();
                var dlg = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), _bindingObject, method);
                _setDelegates[index] = dlg;
            }

            ((Action<T>) _setDelegates[index])(value);
        }

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

        public void RegisterPostSetEvent<T>(string propertyName, Action<T> action)
        {
            _bindingType.GetIndexByPropertyName(propertyName, out var index);
            RegisterPostSetEvent(index, action);
        }

        public void OnPostSet<T>(int index, T value)
        {
            var propertyEvent = GetPropertyEventByIndex(index);
            ((PropertyEvent<T>) propertyEvent)?.PostSetEvent?.Invoke(value);
            _propertyChangeCallback?.Invoke(index);
        }

        public void UnregisterPostSetEvent<T>(int index, Action<T> action)
        {
            var propertyEvent = GetPropertyEventByIndex(index);
            if (propertyEvent != null)
            {
                ((PropertyEvent<T>) propertyEvent).PostSetEvent -= action;
            }
        }

        public void UnregisterPostSetEvent<T>(string propertyName, Action<T> action)
        {
            _bindingType.GetIndexByPropertyName(propertyName, out var index);
            UnregisterPostSetEvent(index, action);
        }

        #region PropertyChangeCallback

        private Action<int> _propertyChangeCallback;
        public Action<int> PropertyChangeCallback => _propertyChangeCallback;

        public void RegisterPropertyChangeCallback(Action<int> callback)
        {
            _propertyChangeCallback += callback;
        }

        public void UnregisterPropertyChangeCallback(Action<int> callback)
        {
            _propertyChangeCallback -= callback;
        }

        public void ClearPropertyChangeCallback()
        {
            _propertyChangeCallback = null;
        }

        #endregion

        public void Clear()
        {
            foreach (var propertyEvent in _propertyEvents)
            {
                propertyEvent?.Dispose();
            }

            if (_bindings != null)
            {
                foreach (var binding in _bindings)
                {
                    binding.Value.UnregisterPropertyChangeCallback(PropertyChangeCallback);
                }

                _bindings.Clear();
            }
        }
    }
}