using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GeoTetra.GTPooling
{
    public static class AddressablesPoolUtility
    {
        public static IResourceLocation GetResourceLocation<T>(object key)
        {
            key = EvaluateKey(key);
            IList<IResourceLocation> locs;
            foreach (var rl in Addressables.ResourceLocators)
            {
                if (rl.Locate(key, typeof(T), out locs))
                    return locs[0];
            }

            return null;
        }
        
        private static object EvaluateKey(object obj)
        {
            if (obj is IKeyEvaluator)
                return (obj as IKeyEvaluator).RuntimeKey;
            return obj;
        }
    }
}

