// using System.Collections.Generic;
// using UnityEngine;
//
// namespace DataBinding.BindingExtensions
// {
//     public class ListBinding<T> : Binding
//     {
//         private BindableList<T> _bindableList;
//
//         private Dictionary<int /*index*/, Binding> _bindings;
//
//         internal ListBinding(BindableList<T> bindingObject) : base(bindingObject)
//         {
//             _bindableList = bindingObject;
//         }
//
//         public int ElementCount => _bindableList.Count;
//
//         public Binding GetElementBinding(int index)
//         {
//             if (ElementCount <= index)
//             {
//                 Debug.LogError($"index {index} is bigger then ElementCount {ElementCount}");
//                 return null;
//             }
//
//             if (typeof(T).IsValueType)
//             {
//                 return null;
//             }
//
//             if (_bindings == null)
//             {
//                 _bindings = new Dictionary<int, Binding>();
//             }
//
//             if (_bindings.TryGetValue(index, out var binding))
//             {
//                 return binding;
//             }
//
//             var element = _bindableList[index];
//
//             if (element == null)
//             {
//                 return null;
//             }
//
//             binding = element.GetBinding();
//             binding.RegisterPropertyChangeCallback(PropertyChangeCallback);
//             return binding;
//         }
//     }
// }