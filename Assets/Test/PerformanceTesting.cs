using System.Reflection;
using DataBinding;
using DataBinding.Test;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;


public class PerformanceTesting : MonoBehaviour
{
    private TestData data;

    private CompareData compareData;

    private Binding binding;

    void Start()
    {
        compareData = new CompareData();
        compareData.StringValue = "0";
        compareData.IntValue = 0;

        data = new DataBinding.Test.TestData();
        binding = new Binding(data);

        data.StringValue = "0";
        data.IntValue = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Profiler.BeginSample("binding set value");
            for (int i = 0; i < 10000; i++)
            {
                binding.SetPropertyValue("StringValue", "666");
            }

            Profiler.EndSample();

            Profiler.BeginSample("after inject direct set value");
            for (int i = 0; i < 10000; i++)
            {
                data.StringValue = "666";
            }

            Profiler.EndSample();

            Profiler.BeginSample("direct set value");
            for (int i = 0; i < 10000; i++)
            {
                compareData.StringValue = "666";
            }

            Profiler.EndSample();
        }
    }
}

