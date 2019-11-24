using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GeoTetra.GTPooling
{
    public static class AddressablesExtensions
    {
        public static async Task<AsyncOperationHandle<GameObject>> PoolInstantiateAsync(this AssetReference reference)
        {
            return await AddressablesPool.InstantiateAsync(reference);
        }
        
        public static async Task<T> PoolInstantiateAsync<T>(this AssetReference reference) where T : Object
        {
            #if UNITY_EDITOR
            Debug.Log("PoolInstantiateAsync " + reference.editorAsset);
            #endif
            AsyncOperationHandle<GameObject> handle = await PoolInstantiateAsync(reference);
            return handle.Result.GetComponent<T>();
        }
    }
}