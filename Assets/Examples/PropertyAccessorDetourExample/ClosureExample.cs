using System;
using UnityEngine;

namespace Examples.PropertyAccessorDetourExample
{
    public class ClosureExample
    {
        private void ExampleMethod()
        {
            var s = "ss";
            Action<string> action = (value) => { Debug.Log(s + value); };

            /*var s2 = "ss";
            Action<string> action2 = (value) => { Debug.Log(s2 + value); };*/
        }
    }
}