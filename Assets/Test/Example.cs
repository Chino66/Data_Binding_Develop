using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MethodInfo indexOf = typeof(string).GetMethod("IndexOf", new Type[] {typeof(char)});
        MethodInfo getByteCount = typeof(Encoding).GetMethod("GetByteCount", new Type[] {typeof(string)});

        Func<string, object, object> indexOfFunc = MagicMethod<string>(indexOf);
        // Func<Encoding, object, object> getByteCountFunc = MagicMethod<Encoding>(getByteCount);

        Debug.Log(indexOfFunc("Hello", 'e'));
        // Debug.Log(getByteCountFunc(Encoding.UTF8, "Euro sign: \u20ac"));
    }

    static Func<T, object, object> MagicMethod<T>(MethodInfo method) where T : class
    {
        // First fetch the generic form
        MethodInfo genericHelper = typeof(Example).GetMethod("MagicMethodHelper",
            BindingFlags.Static | BindingFlags.NonPublic);

        // Now supply the type arguments
        MethodInfo constructedHelper = genericHelper.MakeGenericMethod
            (typeof(T), method.GetParameters()[0].ParameterType, method.ReturnType);

        // Now call it. The null argument is because it's a static method.
        object ret = constructedHelper.Invoke(null, new object[] {method});

        // Cast the result to the right kind of delegate and return it
        return (Func<T, object, object>) ret;
    }

    static Func<TTarget, object, object> MagicMethodHelper<TTarget, TParam, TReturn>(MethodInfo method)
        where TTarget : class
    {
        // Convert the slow MethodInfo into a fast, strongly typed, open delegate
        Func<TTarget, TParam, TReturn> func = (Func<TTarget, TParam, TReturn>) Delegate.CreateDelegate
            (typeof(Func<TTarget, TParam, TReturn>), method);

        // Now create a more weakly typed delegate which will call the strongly typed one
        // Func<TTarget, object, object> ret = (TTarget target, object param) => func(target, (TParam) param);
        Func<TTarget, object, object> ret = (TTarget target, object param) => func(target, (TParam) param);
        return ret;
    }
}