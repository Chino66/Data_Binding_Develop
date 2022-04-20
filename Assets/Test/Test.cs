using System.Reflection;
using DataBinding;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;


public class Test : MonoBehaviour
{
    private TheData data;

    private CompareData compareData;

    private Binding binding;

    void Start()
    {
        compareData = new CompareData();
        compareData.StringValue = "0";
        compareData.IntValue = 0;

        data = new TheData();
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

public class TheData
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
}

public class CompareData
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
}