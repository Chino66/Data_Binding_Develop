using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IIE
{
    public class ImplementInterfaceExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            // var sub = new SubData();
            // sub.Binding = new binding();
            // sub.StringValue = "sub";
            //
            // var data = sub as Data;
            //
            // SetValue(data, "data");

            var data = new Data();

            var sub = data as SubData;
            
            sub.Binding.output();
        }

        public static void SetValue(object instance, string value)
        {
            var binding = instance as IBindable;
            binding.Binding.output();
        }
    }

    public class Data
    {
        public string StringValue;
    }

    public class binding
    {
        public void output()
        {
            Debug.Log("binding output");
        }
    }

    public interface IBindable
    {
        public binding Binding { get; set; }
    }

    public class SubData : Data, IBindable
    {
        public binding Binding { get; set; }
    }
}