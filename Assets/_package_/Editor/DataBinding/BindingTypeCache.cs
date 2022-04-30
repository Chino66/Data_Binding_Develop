using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataBinding
{
    public class BindingTypeCache
    {
        public Type Type;
        public PropertyInfo[] PropertyInfos => _propertyInfos;

        private readonly PropertyInfo[] _propertyInfos;

        public Dictionary<string, int> PropertyName2IndexMap => _propertyName2IndexMap;
        
        private readonly Dictionary<string, int> _propertyName2IndexMap;

        private readonly string[] _propertyNames;

        public BindingTypeCache(Type type)
        {
            this.Type = type;
            _propertyName2IndexMap = new Dictionary<string, int>();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var length = properties.Length;
            _propertyNames = new string[length];
            _propertyInfos = properties;
            for (var i = 0; i < length; i++)
            {
                var property = properties[i];
                var name = property.Name;
                _propertyName2IndexMap.Add(name, i);
                _propertyNames[i] = name;
            }
        }

        public int GetIndexByPropertyName(string propertyName)
        {
            _propertyName2IndexMap.TryGetValue(propertyName, out var index);
            return index;
        }

        public string GetPropertyNameByIndex(int index)
        {
            if (index < 0 || index > _propertyNames.Length)
            {
                return null;
            }

            return _propertyNames[index];
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
    }
}