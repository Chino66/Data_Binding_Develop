using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataBinding.BindingExtensions
{
    public class BindableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        public void Clear()
        {
            Debug.Log($"Clear");
            base.Clear();
        }

        public void Add(TKey key, TValue value)
        {
            Debug.Log($"Add {key}");
            base.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            Debug.Log($"ContainsKey {key}");
            return ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            Debug.Log($"Remove {key}");
            return base.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Debug.Log($"TryGetValue {key} value");
            return base.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                Debug.Log($"get {key} value, value is {base[key]}");
                return base[key];
            }
            set
            {
                Debug.Log($"set {key} value, value is {value}");
                /*
                 * todo 关于值变化通知的方式
                 * 1. 如果字典中的key和value是值类型或string,则可以在取值和赋值的时候知道变化情况
                 * 2. 如果是引用类型,则BindableDictionary需要拿到引用对象的Binding实例
                 *  通过Binding实例的改变事件通知BindableDictionary
                 *  然后再有BindableDictionary向更上一层通知变化
                 *  这一系列的通知关系链是需要明确的
                 */
                base[key] = value;
            }
        }
    }
}