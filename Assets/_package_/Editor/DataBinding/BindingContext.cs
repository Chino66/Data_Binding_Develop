// using System;
// using System.Collections.Generic;
//
// namespace DataBinding
// {
//     public class BindingContext
//     {
//         public object bindingObject;
//         private Dictionary<string /*property name*/, PropertyEvent> _propertyEvents;
//
//         public BindingContext(object bindingObject)
//         {
//             this.bindingObject = bindingObject;
//             _propertyEvents = new Dictionary<string, PropertyEvent>();
//         }
//
//         public void RegisterSetEvent(string propertyName, Action action)
//         {
//             if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
//             {
//                 propertyEvent = new PropertyEvent();
//                 _propertyEvents.Add(propertyName, propertyEvent);
//             }
//
//             propertyEvent.SetEvent.Add(action);
//         }
//
//         public void OnSet(string propertyName)
//         {
//             if (_propertyEvents.TryGetValue(propertyName, out var propertyEvent) == false)
//             {
//                 return;
//             }
//
//             foreach (var action in propertyEvent.SetEvent)
//             {
//                 action?.Invoke();
//             }
//         }
//     }
// }