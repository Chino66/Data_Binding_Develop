using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DataBinding
{
    public class Binding
    {
        private object bindingObject;

        private Dictionary<string /*property name*/, PropertyEvent> _propertyEvents;

        public Binding(object bindingObject)
        {
            this.bindingObject = bindingObject;
            _propertyEvents = new Dictionary<string, PropertyEvent>();
            _binding();
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

        public void RegisterSetEvent(string propertyName, Action<object> action)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                propertyEvent = new PropertyEvent();
                _propertyEvents.Add(propertyName, propertyEvent);
            }

            propertyEvent.SetEvent.Add(action);
        }

        // public void OnSet<T>(string propertyName, T value)
        // {
        //     if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
        //     {
        //         return;
        //     }
        //
        //     foreach (var action in ((PropertyEvent<T>) propertyEvent).SetEvent)
        //     {
        //         action?.Invoke(value);
        //     }
        // }

        public void OnSet(string propertyName, object value)
        {
            if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
            {
                return;
            }
        
            foreach (var action in (propertyEvent).SetEvent)
            {
                action?.Invoke(value);
            }
        }
    }
}