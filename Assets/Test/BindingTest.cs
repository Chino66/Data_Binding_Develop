using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

public class BindingTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var theData = new TheData();
        var binding = new Binding(theData);

        binding.RegisterPostSetEvent<string>(nameof(TheData.StringValue),
            (value) => { Debug.Log($"set value : {value}"); });

        theData.StringValue = "66";
        Debug.Log($"{theData.StringValue}");
    }

    // Update is called once per frame
    void Update()
    {
    }
}