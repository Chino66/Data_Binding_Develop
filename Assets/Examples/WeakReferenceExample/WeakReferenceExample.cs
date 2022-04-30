using System;
using System.Collections;
using System.Collections.Generic;
using DataBinding;
using IIE;
using UnityEngine;

namespace WFE
{
    public class WeakReferenceExample : MonoBehaviour
    {
        private WFEData data;

        void Start()
        {
            data = new WFEData();
            data.StringValue = "66";

            var binding = new Binding(data);
            binding.RegisterPostSetEvent<string>(nameof(WFEData.StringValue),
                (value) => { Debug.Log($"post set value, value is {value}"); });

            data.StringValue = "77";

            // binding.Dispose();
            // BindingCollection.RemoveBinding(data);

            Debug.Log($"BindingCollection.BindingRecord.Count is {BindingCollection.BindingRecord.Count}");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("set value"))
            {
                data.StringValue = "88";
            }

            if (GUILayout.Button("show count"))
            {
                Debug.Log($"BindingCollection.BindingRecord.Count is {BindingCollection.BindingRecord.Count}");
            }
        }
    }

    public class WFEData
    {
        public string StringValue { get; set; }
    }
}