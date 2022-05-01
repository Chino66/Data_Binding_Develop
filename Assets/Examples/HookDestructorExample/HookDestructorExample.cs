using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HDE
{
    public class HookDestructorExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Main();
        }

        private void Main()
        {
            var destructorMethod = typeof(HDEData).GetMethod("Finalize",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Debug.Log($"{destructorMethod.Name}");
            //
            // Debug.Log($"Finalize is null {destructorMethod == null}");

            var methods =
                typeof(HDEData).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (var method in methods)
            {
                Debug.Log(method.Name);
            }
        }
    }


    public class HDEData
    {
        public string StringValue;

        // ~HDEData()
        // {
        // }
    }
}