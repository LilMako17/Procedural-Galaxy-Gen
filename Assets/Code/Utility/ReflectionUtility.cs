using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A small object for coupling an instance of an Attribute with a Type.
/// </summary>
public class AttributeTypePair<T> where T : Attribute
{
    public readonly T Attribute;
    public readonly Type Type;

    public AttributeTypePair(T attribute, Type type)
    {
        Attribute = attribute;
        Type = type;
    }
}

/// <summary>
/// A collection of utility methods for working with System.Reflection.
/// </summary>
public static class ReflectionUtility
{
    /// <summary>
    /// Cached reference to current types in all assemblies.
    /// </summary>
    private static List<Type> _currentTypes;

    /// <summary>
    /// Gets all the types in the currently loaded assemblies.
    /// </summary>
    public static IEnumerable<Type> GetCurrentTypes()
    {
        ThrowIfNotEditorOrDebug();
        return _currentTypes ?? (_currentTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).ToList());
    }

    /// <summary>
    /// Find types decorated with an Attribute T and return a tuple of the Attribute and Type.
    /// </summary>
    public static IEnumerable<AttributeTypePair<T>> GetTypesWithAttribute<T>() where T : Attribute
    {
        ThrowIfNotEditorOrDebug();
        var attributeType = typeof(T);
        return GetCurrentTypes()
            .Select(type =>
            {
                var attrs = type.GetCustomAttributes(attributeType, false);
                if (attrs.Length > 0)
                {
                    return new AttributeTypePair<T>((T)attrs[0], type);
                }
                else
                {
                    return null;
                }
            })
            .Where(obj => obj != null);
    }

    /// <summary>
    /// Find all types that implement an interface, filtering out the interface itself.
    /// </summary>
    public static IEnumerable<Type> GetTypesOfInterface<T>() where T : class
    {
        ThrowIfNotEditorOrDebug();
        var interfaceType = typeof(T);
        if (!interfaceType.IsInterface)
        {
            throw new Exception($"Attempting to GetTypesOfInterface but provided type \"{interfaceType}\" is not an interface");
        }
        return GetCurrentTypes()
            .Where(p => interfaceType.IsAssignableFrom(p))
            .Where(type => !type.IsInterface);
    }

    private static void ThrowIfNotEditorOrDebug()
    {
        if (!Application.isEditor && !Debug.isDebugBuild)
        {
            UnityEngine.Debug.LogError("ReflectionUtility should not be used outside of the editor or Development Mode builds.");
            throw new Exception("ReflectionUtility should not be used outside of the editor or Development Mode builds.");
        }
    }
}
