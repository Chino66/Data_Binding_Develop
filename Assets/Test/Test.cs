using System.Reflection;
using DataBinding;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;


public class Test : MonoBehaviour
{
    private TheData data;

    private Binding binding;

    private View _view;

    void Start()
    {
        data = new TheData();
        binding = new Binding(data);

        _view = new View(data);
        _view.Text = "change string";

        // BindingCollection.RegisterBinding(typeof(TheData));
        data.StringValue = "0";
        data.IntValue = 0;

        // binding.RegisterPostSetEvent<string>("StringValue", (value) =>
        // {
        //     Debug.Log($"set event StringValue value is {value}");
        //     _view.Text = value.ToString();
        // });
        //
        // binding.RegisterPostSetEvent<int>("IntValue",
        //     (value) => { Debug.Log($"set event IntValue value is {value}"); });
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

            Profiler.BeginSample("direct set value");
            for (int i = 0; i < 10000; i++)
            {
                data.StringValue = "666";
            }

            Profiler.EndSample();
        }
    }
}

public class View
{
    private string _text;

    private TheData _data;

    public View(TheData data)
    {
        _data = data;
    }

    public string Text
    {
        get { return _text; }
        set
        {
            _text = value;
            OnChange(value);
        }
    }

    public void OnChange(string value)
    {
        Debug.Log($"OnChange {value}");
        if (_data.StringValue != value)
        {
            _data.StringValue = value;
        }
    }
}

public class TheData
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
}