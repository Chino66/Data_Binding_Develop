using System;
using System.Linq;
using System.Reflection;
using _package_.Editor;
using DataBinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Test.Editor
{
    public class TestWindow : EditorWindow
    {
        [MenuItem("Tool/Test Window")]
        static void Open()
        {
            var window = GetWindow<TestWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            rootVisualElement.Add(GetUI());
        }

        private TheData data;

        private VisualElement GetUI()
        {
            data = new TheData();
            var binding = new Binding(data);

            var elementBinding = new UIElementBinding(binding);
            /*todo 借用bindingPath作为属性绑定依据 后续考虑其他方式*/
            var root = new VisualElement();
            var strLab = new Label();
            strLab.text = "label";
            root.Add(strLab);
            strLab.bindingPath = "StringValue";
            elementBinding.Bind(strLab);

            var intLab = new Label();
            intLab.text = "int";
            root.Add(intLab);
            intLab.bindingPath = "IntValue";
            elementBinding.Bind(intLab);

            var boolLab = new Label();
            boolLab.text = "bool";
            root.Add(boolLab);
            boolLab.bindingPath = "BoolValue";
            elementBinding.Bind(boolLab);

            var boolTog = new Toggle();
            boolTog.text = "boolTog";
            boolTog.value = false;
            root.Add(boolTog);
            boolTog.bindingPath = "BoolValue";
            elementBinding.Bind(boolTog);

            var button = new Button();
            button.text = "random label";
            root.Add(button);
            button.clicked += () =>
            {
                strLab.text = Random.Range(0, 100).ToString();
                intLab.text = Random.Range(100, 200).ToString();
                boolLab.text = data.IntValue % 2 == 1 ? "true" : "false";
                boolTog.text = boolLab.text;
                boolTog.value = bool.Parse(boolLab.text);
            };

            var button2 = new Button();
            button2.text = "random data";
            root.Add(button2);
            button2.clicked += () =>
            {
                data.StringValue = Random.Range(0, 100).ToString();
                data.IntValue = Random.Range(100, 200);
                data.BoolValue = data.IntValue % 2 == 0 ? true : false;
            };

            var button3 = new Button();
            button3.text = "show data";
            root.Add(button3);
            button3.clicked += () =>
            {
                Debug.Log($"data.StringValue:{data.StringValue}");
                Debug.Log($"data.IntValue:{data.IntValue}");
                Debug.Log($"data.BoolValue:{data.BoolValue}");
            };

            /*var button4 = new Button();
            button4.text = "show lab generic type";
            root.Add(button4);
            button4.clicked += () =>
            {
                var type = lab.GetType();
                var interfaces = type.GetInterfaces();
                foreach (var itf in interfaces)
                {
                    if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(INotifyValueChanged<>))
                    {
                        foreach (var argument in itf.GenericTypeArguments)
                        {
                            Debug.Log("     " + argument.Name);
                        }
                    }
                }
            };*/

            return root;

            /*数据绑定手写代码示例*/
            // var valueChanged = (INotifyValueChanged<string>) lab;
            // /*数据绑定组件*/
            // binding.RegisterPostSetEvent<string>("StringValue", o => { valueChanged.value = o; });
            //
            // /*组件绑定数据*/
            // valueChanged.RegisterValueChangedCallback(evt =>
            // {
            //     if (data.StringValue != evt.newValue)
            //     {
            //         data.StringValue = evt.newValue;
            //     }
            // });
        }

        /*private void BindExample(Binding binding, object element)
        {
            /*1. 组件是否有BindableElement,有则获取绑定属性名称#1#
            var bindable = element as BindableElement;
            if (bindable == null)
            {
                Debug.LogError("bindable is null");
                return;
            }

            var propertyName = bindable.bindingPath;

            /*2. 组件是否实现INotifyValueChanged<>接口,有则获取泛型类型#1#
            var type = element.GetType();
            var interfaces = type.GetInterfaces();
            var genericType =
                (from itf in interfaces
                    where itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(INotifyValueChanged<>)
                    select itf.GenericTypeArguments[0]).FirstOrDefault();

            if (genericType == null)
            {
                Debug.LogError("genericType is null");
                return;
            }

            Debug.Log($"genericType {genericType.Name}");

            /*3. 数据类型#1#
            var propertyInfo = binding.BindingObject.GetType()
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null)
            {
                Debug.LogError("propertyInfo is null");
                return;
            }

            var propertyType = propertyInfo.PropertyType;

            Debug.Log($"propertyType {propertyType.Name}");
            if (propertyType == genericType)
            {
                var method = this.GetType()
                    .GetMethod(nameof(_bindSameType), BindingFlags.Instance | BindingFlags.NonPublic);
                method = method.MakeGenericMethod(propertyType);
                method.Invoke(this, new[] {binding, propertyName, element});
            }
            else
            {
                var method = this.GetType()
                    .GetMethod(nameof(_bindDiffType), BindingFlags.Instance | BindingFlags.NonPublic);
                method = method.MakeGenericMethod(propertyType, genericType);
                method.Invoke(this, new[] {binding, propertyName, element});
            }
        }

        private void _bindSameType<T>(Binding binding, string propertyName, INotifyValueChanged<T> valueChanged)
        {
            /*数据绑定组件#1#
            binding.RegisterPostSetEvent<T>(propertyName, o => { valueChanged.value = o; });

            /*组件绑定数据#1#
            valueChanged.RegisterValueChangedCallback(evt =>
            {
                var value = binding.GetPropertyValue<T>(propertyName);
                if (value == null || value.Equals(evt.newValue) == false)
                {
                    binding.SetPropertyValue(propertyName, evt.newValue);
                }
            });
        }

        private void _bindDiffType<T, T2>(Binding binding, string propertyName, INotifyValueChanged<T2> valueChanged)
        {
            /*数据绑定组件#1#
            binding.RegisterPostSetEvent<T>(propertyName,
                o => { valueChanged.value = (T2) Convert.ChangeType(o, typeof(T2)); });

            /*组件绑定数据#1#
            valueChanged.RegisterValueChangedCallback(evt =>
            {
                // var method = typeof(T).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public,);
                var method = typeof(T).GetMethod("Parse", new[] {typeof(string)});
                T value = default;
                if (method != null)
                {
                    value = (T) method.Invoke(null, new object[] {evt.newValue});
                }
                else
                {
                    value = (T) Convert.ChangeType(evt.newValue, typeof(T));
                }

                if (value == null || binding.GetPropertyValue<T>(propertyName).Equals(value) == false)
                {
                    binding.SetPropertyValue(propertyName, value);
                }
            });
        }*/
    }
}