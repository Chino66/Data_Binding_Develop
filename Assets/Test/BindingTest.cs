using System;
using DataBinding;
using DataBinding.Test;
using UnityEngine;
using Random = UnityEngine.Random;

public class BindingTest : MonoBehaviour
{
    private string content = "";
    private TestData theData;

    void Start()
    {
        theData = new TestData();
        theData.SubData = new SubData();
        var binding = theData.GetBinding();

        binding.RegisterPostSetEvent<string>(nameof(TestData.StringValue),
            (value) =>
            {
                Debug.Log("set TestData.StringValue property");
                Debug.Log($"set value : {value}");
                content += $"set value : {value}\n";
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
            theData.StringValue = Random.Range(0, 99).ToString();
            Debug.Log($"{theData.StringValue}");
        }

        GUILayout.Label(content);
    }
}