using UnityEngine;
using System.Reflection;
using NUnit.Framework;

public static class TestImmitationHelper
{
    public static void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName,BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.IsNotNull(method, $"{methodName} was not found on {target.GetType().Name}");

        method.Invoke(target, null);
    }

    public static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName,BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.IsNotNull(field, $"{fieldName} was not found on {target.GetType().Name}");

        field.SetValue(target, value);
    }
}
