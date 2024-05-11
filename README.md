# DataBinding

项目地址：[https://github.com/Chino66/Data\_Binding\_Develop](https://github.com/Chino66/Data_Binding_Develop)

基于MonoMod的Detour技术和Mono.Cecil的代码注入实现真正的数据绑定。

注意：功能实现基于Mono，所以在编译IL2CPP后无法使用，所以这个库适用于Unity编辑器上和Windows平台使用。

## 使用方法

示例：

    // 数据类
    public class Data{
      public string Value {get;set;} // 必须是属性
    }
    
    // 创建数据实例
    var data = new Data();
    // 获取data的绑定实例
    var binding = data.GetBinding();
    
    // 注册Value属性的值修改后事件监听
    binding.RegisterPostSetEvent<string>(nameof(Data.Value), (value) =>
    {
        Debug.Log($"Post Set value:{value}");
    });
    // 修改data.Value触发事件
    data.Value = "chino66";

## 特点

通过上面的例子可以看出，我们并没有对Data类的字段做任何处理就能实现对Value值的修改并触发事件。

以往为了实现这样的功能必然会对Data的属性进行添加事件监听，如下：

    public class Data
    {
      public Action<string> OnValueChange;
      private string _value;
      public string Value 
      {
        get => _value;
        set
        {
          _value = value;
          OnValueChange?.Invoke(_value);
        }
      }
    }
    var data = new Data();
    data.OnValueChange += (value) =>
    {
        Debug.Log($"Set value:{value}");
    }
    data.Value = "chino66";
    

需要写一段重复且繁琐的代码才能实现上面的功能，这就是本插件提供的第一个能力，快速实现数据绑定。

虽然可以通过对Data添加如\[AutoGenerateAction\]的自定义特性，然后使用SourceGenerator在运行时生成代码来解决手写重复代码的问题（这也很重要，尤其在运行时环境），但如果你想要给一些第三方库中的数据添加绑定，则生成代码就失效了，因为你不能对这些这些库的类进行任何修改，所以只能将它们封装一个装饰类去监听。而本插件基于MonoMod的Detour技术可以实现动态修改对第三方库进行直接数据绑定。

## 原理

其实插件做的事情和上面的代码原理是一样的也是通过一个OnValueChange响应修改，只不过插件使用了运行时DynamicMethod（动态方法）以及Detour（改道）技术帮助我们做了这些事情。

**本文只介绍原理，不会涉及任何细节，关于细节实现请看源码。**

### 关于属性Property

Property其实是一个语法糖，形如`public string Value {set;get}`在编译后真正的样子是类似：

    private string _value;
    
    void set_StringValue(string value)
    {
      _value = value;
    }
    string get_StringValue()
    {
      return _value;
    }

Value属性会变成一个\_value字段和set\_StringValue与get\_StringValue两个方法。

所以上面绑定代码类似是：

    private string _value;
    public Action<string> OnValueChange;
    void set_StringValue(string value)
    {
      _value = value;
      OnValueChange?.Invoke(_value)
    }

### 对属性方法Detour（改道）

从上面的例子我们知道属性的get和set本质是方法，所以想要自动实现绑定，只要将set指向的方法改成我们指定的方法就行，这需要两个技术就是上面提到的DynamicMethod（动态方法）以及Detour（改道）。

#### DynamicMethod动态方法

动态方法允许运行时生成方法，我们需要生成一个包含Action<T>回调的方法，如：

    void set_StringValue_Dynamic(string value)
    {
      _value = value;
      OnValueChange?.Invoke(_value)
    }

#### Detour改道

改道的意思就是将方法指针从一个方法指向另一个方法，在这里我们将原本set\_StringValue指向的方法改成指向set\_StringValue\_Dynamic，这样我们就能实现当Value被设置时调用我们生成的方法实现绑定。

**具体细节请参看源码。**