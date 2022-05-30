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
            this.data = new WFEData();
            this.data.StringValue = "99";
            var binding = this.data.GetBinding();

            var data = new WFEData();
            data.StringValue = "66";
            binding = data.GetBinding();
            binding.RegisterPostSetEvent<string>(nameof(WFEData.StringValue),
                (value) => { Debug.Log($"post set value, value is {value}"); });
            data.StringValue = "77";

            data = new WFEData();
            data.StringValue = "88";
            binding = data.GetBinding();

            data = new WFEData();
            data.StringValue = "99";
            binding = data.GetBinding();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("set value"))
            {
                data.StringValue = "88";
            }

            if (GUILayout.Button("show count"))
            {
                Debug.Log($"BindingCollection.BindingRecord.Count is {BindingCollection.BindingObjectRecordCount()}");
            }

            if (GUILayout.Button("GC"))
            {
                GC.Collect();
            }

            if (GUILayout.Button("Remove"))
            {
                // BindingCollection.BindingObjectRecord.Remove(data);
            }
        }
    }

    public class WFEData
    {
        public string StringValue { get; set; }
    }
}