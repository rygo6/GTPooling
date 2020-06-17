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
    
    public AssetReferenceComponentRestriction(Type componentType)
    {
        _componentType = componentType;
    }
    
    public override bool ValidateAsset(Object obj)
    {
        var path = AssetDatabase.GetAssetOrScenePath(obj);
        return ValidateAsset(path);
    }
    
    public override bool ValidateAsset(string path)
    {
        return AssetDatabase.LoadAssetAtPath(path, _componentType) != null;
    }
}
