using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEngine;

[AssetReferenceSurrogate(typeof(AssetReferenceComponentRestriction))]
public class AssetReferenceComponentRestrictionSurrogate : AssetReferenceUIRestrictionSurrogate
{
    AssetReferenceComponentRestriction data;

    public override void Init(AssetReferenceUIRestriction initData)
    {
        data = initData as AssetReferenceComponentRestriction;
    }
    
    public override bool ValidateAsset(Object obj)
    {
        var path = AssetDatabase.GetAssetOrScenePath(obj);
        return ValidateAsset(path);
    }

    public override bool ValidateAsset(string path)
    {
        return AssetDatabase.LoadAssetAtPath(path, data.ComponentType) != null;
    }
    
    public override string ToString()
    {
        return data.ToString();
    }
}