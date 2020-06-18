using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Used to restrict an AssetReference field or property to only allow items wil specific labels.  This is only enforced through the UI.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class AssetReferenceComponentRestriction : AssetReferenceUIRestriction
{
    private Type _componentType;
    
    public Type ComponentType => _componentType;
    
    public AssetReferenceComponentRestriction(Type componentType)
    {
        _componentType = componentType;
    }
}
