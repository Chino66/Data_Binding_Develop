using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataBinding
{
    public class BindingTypeCache
    {
        public Type Type;
        public PropertyInfo[] PropertyInfos => _propertyInfos;

        private PropertyInfo[] _propertyInfos;
        private Dictionary<string, int> _propertyName2IndexMap;

        public BindingTypeCache(Type type)
        {
            this.Type = type;
            _propertyName2IndexMap = new Dictionary<string, int>();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var lenght = properties.Length;
            _propertyInfos = properties;
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                _propertyName2IndexMap.Add(property.Name, i);
            }
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
    }
}