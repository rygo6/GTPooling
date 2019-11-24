using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GeoTetra.GTPooling
{
    public class AddressablesPool
    {
        private static Dictionary<int, Object> _objectsInUse = new Dictionary<int, Object>();
        private static OrderedDictionary _releasedObjects = new OrderedDictionary();

        public static async Task<AsyncOperationHandle<GameObject>> InstantiateAsync(string key)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, trackHandle: true);
            await handle.Task;
            return handle;
        }

        public static async Task<AsyncOperationHandle<GameObject>> InstantiateAsync(AssetReference reference)
        {
            AsyncOperationHandle<GameObject> handle = await InstantiateAsync(reference.RuntimeKey as string);
            return handle;
        }
        
//        public static async Task<T> PoolInstantiateAsync<T>(this AssetReference reference) where T : Object
    }
}

