using System;
using DataBinding;
using DataBinding.Test;
using UnityEngine;

public class BindingTest : MonoBehaviour
{
    void Start()
    {
        var theData = new TestData();
        theData.SubData = new SubData();
        var binding = new Binding(theData);

        binding.RegisterPostSetEvent<string>(nameof(TestData.StringValue),
            (value) =>
            {
                Debug.Log("set TestData.StringValue property");
                Debug.Log($"set value : {value}");
            });

        theData.StringValue = "66";
        Debug.Log($"{theData.StringValue}");

        theData.SubData.StringValue = "77";
        Debug.Log($"{theData.SubData.StringValue}");


        /*binding.RegisterPostSetEvent<SubData>(nameof(TestData.SubData),
            (value) =>
            {
                Debug.Log("set TestData.SubData property");
                Debug.Log($"set value : {value}");
            });*/

        // theData.SubData = new SubData();
        // theData.SubDataBindable = new SubDataBindable();

        /*var subBinding = binding.FindBinding(nameof(TestData.SubDataBindable));
            
        subBinding.RegisterPostSetEvent<string>(
            nameof(SubDataBindable.StringValue),
            (value) =>
            {
                Debug.Log("set SubData.StringValue property");
                Debug.Log($"set value : {value}");
            });

        theData.SubDataBindable.StringValue = "99";*/

        /*var binging = binding.FindBinding(nameof(TestData.SubDataBindable));
        binging.OnPostSet(1, "999");*/


        // Debug.Log(BindingCollection.Information());
    }

    private void OnGUI()
    {
        if (GUILayout.Button("output Binding count"))
        {
            
        }
    }
}