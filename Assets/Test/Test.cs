using DataBinding;
using UnityEngine;
using UnityEngine.UIElements;


public class Test : MonoBehaviour
{
    private TheData data;

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

    private View _view;

    void Start()
    {
        data = new TheData();
        var binding = new Binding(data);

        _view = new View(data);
        _view.Text = "change string";

        // BindingCollection.RegisterBinding(typeof(TheData));
        data.StringValue = "0";
        data.IntValue = 0;

        binding.RegisterSetEvent("StringValue", (value) =>
        {
            Debug.Log($"set event StringValue value is {value}");
            _view.Text = value.ToString();
        });

        binding.RegisterSetEvent("IntValue", (value) => { Debug.Log($"set event IntValue value is {value}"); });
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnGUI()
    {
        if (GUILayout.Button(_view.Text))
        {
            data.StringValue = Random.Range(0, 100).ToString();
        }

        if (GUILayout.Button("change view"))
        {
            _view.Text = "change view";
        }

        // if (GUILayout.Button("change int"))
        // {
        //     data.IntValue = Random.Range(0, 100);
        // }
    }
}

public class TheData
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
}