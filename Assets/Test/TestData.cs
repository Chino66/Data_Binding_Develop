using System;
using UnityEngine;

namespace DataBinding.Test
{
    public class TestData
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }

        public SubData SubData { get; set; }

        public override string ToString()
        {
            var content = $"SubData StringValue is {StringValue}, IntValue is {IntValue}, BoolValue is {BoolValue}";
            content += $"    SubDataBindable is {SubData.ToString()}";
            return content;
        }
    }

    public class SubData
    {
        public string SubDataBindableStringValue { get; set; }
        public string StringValue { get; set; }

        public override string ToString()
        {
            return
                $"SubDataBindable SubDataBindableStringValue is {SubDataBindableStringValue}, StringValue is {StringValue}";
        }
    }

    public class CompareData
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }
    }
}