// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace DataBinding.BindingExtensions
// {
//     public static class BindableListCollection
//     {
//         private Dictionary<BindableList, ListBinding> _bindings;
//         public static void BindableList_Add<T>(BindableList<T> instance, T item)
//         {
//             // 通过_bindings找到instance的binding,然后调用对应的方法,添加元素
//             
//             // list本质和数据类的字段是一样的,只不过list的下标就是字段的名字
//         }
//     }
//
//     public class BindableList<T> : List<T>, IList<T>
//     {
//         public void Add(T item)
//         {
//             base.Add(item);
//
//             BindableListCollection.BindableList_Add(this, item);
//         }
//
//         public void Clear()
//         {
//             base.Clear();
//         }
//
//         public bool Contains(T item)
//         {
//             return base.Contains(item);
//         }
//
//         public void CopyTo(T[] array, int arrayIndex)
//         {
//             base.CopyTo(array, arrayIndex);
//         }
//
//         public bool Remove(T item)
//         {
//             return base.Remove(item);
//         }
//
//         public int IndexOf(T item)
//         {
//             return base.IndexOf(item);
//         }
//
//         public void Insert(int index, T item)
//         {
//             base.Insert(index, item);
//         }
//
//         public void RemoveAt(int index)
//         {
//             base.RemoveAt(index);
//         }
//
//         public T this[int index]
//         {
//             get => base[index];
//             set => base[index] = (T) value;
//         }
//     }
// }