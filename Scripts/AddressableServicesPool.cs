using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

namespace GeoTetra.GTPooling
{
    public class AddressableServicesPool : MonoBehaviour
    {
        [SerializeField] private bool _isGlobalPool;
        [SerializeField] List<AssetReference> _prePoolReferences = new List<AssetReference>();
        [SerializeField] private UnityEvent _prePoolingComplete;

        public static AddressableServicesPool GlobalPool;
        
        private Dictionary<string, ScriptableObject>  _pooledServices = new Dictionary<string, ScriptableObject>();

        private void Awake()
        {
            if (_isGlobalPool)
            {
                if (GlobalPool != null)
                {
                    Debug.LogError("Trying to set GlobalPool twice!");
                }
                GlobalPool = this;
            }

            PrePool();
        }

        private async void PrePool()
        {
            for (int i = 0; i < _prePoolReferences.Count; ++i)
            {
               await LoadAsync(_prePoolReferences[i]);
            }
            _prePoolingComplete.Invoke();
        }
        
        public T PrePooledPopulate<T>() where T : ScriptableObject
        {
            IResourceLocation location = AddressablesPoolUtility.GetGameObjectResourceLocation<ScriptableObject>(typeof(T).Name);
            if (location == null)
            {
                Debug.LogError("Reference not set " + typeof(T));
                return null;
            }

            return PrePooledPopulate<T>(location);
        }
        
        public T PrePooledPopulate<T>(string key) where T : ScriptableObject
        {
            IResourceLocation location = AddressablesPoolUtility.GetGameObjectResourceLocation<ScriptableObject>(key);
            if (location == null)
            {
                Debug.LogError("Reference not set " + key.ToString());
                return null;
            }

            return PrePooledPopulate<T>(location);
        }

        public T PrePooledPopulate<T>(AssetReference reference) where T : ScriptableObject
        {
            IResourceLocation location = AddressablesPoolUtility.GetGameObjectResourceLocation<ScriptableObject>(reference.RuntimeKey);
            if (location == null)
            {
                Debug.LogError("Reference not set " + reference.ToString());
                return null;
            }
            
            return PrePooledPopulate<T>(location);         
        }
        
        private T PrePooledPopulate<T>(IResourceLocation location) where T : ScriptableObject
        {
            if (_pooledServices.TryGetValue(location.PrimaryKey, out ScriptableObject pooledService))
            {
                return (T) pooledService;
            }
            else
            {
                Debug.LogWarning($"MultiUser Pooled Reference not found {location.PrimaryKey}");
                return null;
            }
        }
        
        private async Task LoadAsync(AssetReference reference)
        {
            IResourceLocation location = AddressablesPoolUtility.GetGameObjectResourceLocation<ScriptableObject>(reference.RuntimeKey);

            //If location is initially null, means the internal ResourceManager has not alloced, so instantiate
            //by string key to trigger that.
            if (location == null)
            {
                AsyncOperationHandle<ScriptableObject> handle = Addressables.LoadAssetAsync<ScriptableObject>(reference.RuntimeKey);
                await handle.Task;
                location = AddressablesPoolUtility.GetGameObjectResourceLocation<ScriptableObject>(reference.RuntimeKey);
                Debug.Log($"Adding Service {location.PrimaryKey}");
                _pooledServices.Add(location.PrimaryKey, handle.Result);
                return;
            }
            
            ScriptableObject scriptableObject = RetrieveObject(location.PrimaryKey);
            if (scriptableObject == null)
            {
                AsyncOperationHandle<ScriptableObject> handle = Addressables.LoadAssetAsync<ScriptableObject>(location);
                await handle.Task;
                Debug.Log($"Adding Service {location.PrimaryKey}");
                _pooledServices.Add(location.PrimaryKey, handle.Result);
            }
        }

        private ScriptableObject RetrieveObject(string primaryKey)
        {
            return _pooledServices.TryGetValue(primaryKey, out ScriptableObject scriptableObject) ? scriptableObject : null;
        }
    }
}

