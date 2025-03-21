﻿using System.Reflection;
using Nextended.Blazor.Helper;
using Nextended.Core.Helper;

namespace MudBlazor.Extensions.Helper.Internal;

internal static class PropertyHelper
{
    public static string GetDescription(this Enum value)
        => Nextended.Core.Helper.EnumExtensions.ToDescriptionString(value);

    public static bool IsPropertyPathSubPropertyOf(string propertyPath, string path)
    {
        if (string.IsNullOrWhiteSpace(propertyPath) || string.IsNullOrWhiteSpace(path))
            return false;
        var pathParts = propertyPath.Split('.');
        var subPathParts = path.Split('.');
        return pathParts.Length >= subPathParts.Length && !subPathParts.Where((t, i) => pathParts[i] != t).Any();
    }
    
    public static IDictionary<string, object> ValidValuesDictionary<T>(Action<T> componentOptions, bool removeDefaults) where T : new()
    {
        return ValuesDictionary(componentOptions, removeDefaults)
            .Where(kvp => ComponentRenderHelper.IsValidParameter(typeof(T), kvp.Key, kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static IDictionary<string, object> ValidValuesDictionary<T>(T dialog, bool removeDefaults) where T : new()
    {
        return ValuesDictionary(dialog, removeDefaults)
            .Where(kvp => ComponentRenderHelper.IsValidParameter(typeof(T), kvp.Key, kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 
    }
    
    public static IDictionary<string, object> ValuesDictionary<T>(Action<T> options, bool removeDefaults,
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) where T : new()
    {
        return DictionaryHelper.GetValuesDictionary<T>(options, removeDefaults, flags);
    }

    public static IDictionary<string, object> ValuesDictionary<T>(bool removeDefaults, params Action<T>[] options)
        where T : new()
    {
        return DictionaryHelper.GetValuesDictionary<T>(removeDefaults, options);
    }

    public static IDictionary<string, object> ValuesDictionary<T>(T o, bool removeDefaults, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        where T : new()
    {
        return DictionaryHelper.GetValuesDictionary<T>(o, removeDefaults, flags);
    }

    public static object CopyPropertyValuesTo(this object source, object target)
    {
        if (target == null || source == null) return target;

        var targetType = target.GetType();
        var sourceType = source.GetType();

        foreach (var property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite) continue;

            var sourceProperty = sourceType.GetProperty(property.Name);
            if (sourceProperty == null) continue;

            var sourceValue = sourceProperty.GetValue(source);

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var targetValue = property.GetValue(target);
                if (targetValue == null)
                {
                    targetValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(target, targetValue);
                }
                CopyPropertyValuesTo(sourceValue, targetValue);
            }
            else
            {
                property.SetValue(target, sourceValue);
            }
        }

        return target;
    }
}