using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace DataBinding
{
    public class Binding : IDisposable
    {
        private object _bindingObject;
        public object BindingObject => _bindingObject;
        private readonly PropertyInfo[] _propertyInfos;
        private readonly PropertyEvent[] _propertyEvents;
        private readonly Delegate[] _setDelegates;
        private readonly Delegate[] _getDelegates;
        private readonly BindingTypeCache _bindingType;

        public Binding(object bindingObject)
        {
            _bindingObject = bindingObject;

            _bindingType = BindingCollection.MakeBinding(_bindingObject, this);

            _propertyInfos = _bindingType.PropertyInfos;
            var lenght = _propertyInfos.Length;
            _propertyEvents = new PropertyEvent[lenght];
            _setDelegates = new Delegate[lenght];
            _getDelegates = new Delegate[lenght];
        }

        public int GetIndexByPropertyName(string propertyName)
        {
            return _bindingType.GetIndexByPropertyName(propertyName);
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
            var index = _bindingType.GetIndexByPropertyName(propertyName);
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
            var index = _bindingType.GetIndexByPropertyName(propertyName);
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

            ((Func<T>) _getDelegates[index])();

            return default;
        }


        public void SetPropertyValue<T>(string propertyName, T value)
        {
            var index = _bindingType.GetIndexByPropertyName(propertyName);
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
            var index = _bindingType.GetIndexByPropertyName(propertyName);
            RegisterPostSetEvent(index, action);
        }

        internal void OnPostSet<T>(int index, T value)
        {
            var propertyEvent = GetPropertyEventByIndex(index);
            ((PropertyEvent<T>) propertyEvent)?.PostSetEvent?.Invoke(value);
        }

        #region Dispose

        private bool _disposed;

        /// <summary>
        /// 需要调用此方法才能将数据实例释放,否则将一直被BindingCollection持有引用
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var pair in _propertyEvents)
                {
                    pair.Dispose();
                }

                for (int i = 0; i < _setDelegates.Length; i++)
                {
                    _setDelegates[i] = null;
                }

                for (int i = 0; i < _getDelegates.Length; i++)
                {
                    _getDelegates[i] = null;
                }

                BindingCollection.RemoveBinding(_bindingObject);

                _bindingObject = null;
            }

            _disposed = true;
        }

        ~Binding()
        {
            Dispose(true);
            Debug.Log("when binding destructor");
        }

        #endregion
    }
}