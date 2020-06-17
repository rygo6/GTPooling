using System.Collections.Generic;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GeoTetra.GTPooling
{
    /// <summary>
    /// This Pool holds references ServiceBehaviours.
    /// </summary>
    public class AddressableServicesPool : MonoBehaviour
    {
        [SerializeField] private bool _isGlobalPool;
        
        [SerializeField] 
        [AssetReferenceUILabelRestriction("Service")]
        List<AssetReference> _prePoolReferences = new List<AssetReference>();
        
        [SerializeField] private UnityEvent _prePoolingComplete;

        public static AddressableServicesPool GlobalPool;

        private Dictionary<string, ServiceBehaviour>  _pooledServices = new Dictionary<string, ServiceBehaviour>();

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
               await LoadServiceAsync(_prePoolReferences[i]);
            }
            _prePoolingComplete.Invoke();
        }

        public T PrePooledPopulate<T>() where T : ServiceBehaviour
        {
            IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<GameObject>(typeof(T).Name);
            if (location == null)
            {
                Debug.LogError("Reference not set " + typeof(T));
                return null;
            }

            return PrePooledPopulate<T>(location);
        }
        
        public T PrePooledPopulate<T>(string key) where T : ServiceBehaviour
        {
            IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<GameObject>(key);
            if (location == null)
            {
                return PrePooledPopulate<T>();
            }

            return PrePooledPopulate<T>(location);
        }

        public T PrePooledPopulate<T>(AssetReference reference) where T : ServiceBehaviour
        {
            IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<GameObject>(reference.RuntimeKey);
            if (location == null)
            {
                return PrePooledPopulate<T>();
            }
            
            return PrePooledPopulate<T>(location);         
        }
        
        private T PrePooledPopulate<T>(IResourceLocation location) where T : ServiceBehaviour
        {
            if (_pooledServices.TryGetValue(location.PrimaryKey, out ServiceBehaviour pooledService))
            {
                return (T) pooledService;
            }
            else
            {
                Debug.LogWarning($"MultiUser Pooled Reference not found {location.PrimaryKey}");
                return null;
            }
        }
        
        private async Task LoadServiceAsync(AssetReference reference)
        {
            IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<GameObject>(reference.RuntimeKey);

            //If location is initially null, means the internal ResourceManager has not alloced, so instantiate
            //by string key to trigger that.
            if (location == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(reference.RuntimeKey, Vector3.zero, Quaternion.identity, transform);
                await handle.Task;
                location = AddressablesPoolUtility.GetResourceLocation<GameObject>(reference.RuntimeKey);
                AddServiceToPool(location, handle, reference);
                return;
            }
            
            ServiceBehaviour service = RetrieveObject(location.PrimaryKey);
            if (service == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(location, Vector3.zero, Quaternion.identity, transform);
                await handle.Task;
                AddServiceToPool(location, handle, reference);
            }
        }

        private void AddServiceToPool(IResourceLocation location, AsyncOperationHandle<GameObject> handle, AssetReference reference)
        {
            _serviceBehavioursRecyclable.Clear();
            handle.Result.GetComponents(_serviceBehavioursRecyclable);
            
            #if UNITY_EDITOR
            if (_serviceBehavioursRecyclable.Count > 1)
            {
                Debug.LogWarning($"Service {reference.editorAsset.name} has too many ServiceBehaviours on it, should only be one.");
                return;
            }
            if (_serviceBehavioursRecyclable.Count == 0)
            {
                Debug.LogWarning($"Service {reference.editorAsset.name} has too many ServiceBehaviours on it, should only be one.");
                return;
            }
            #endif
            
            _pooledServices.Add(location.PrimaryKey, _serviceBehavioursRecyclable[0]);
        }

        private List<ServiceBehaviour> _serviceBehavioursRecyclable = new List<ServiceBehaviour>();
        
        private ServiceBehaviour RetrieveObject(string primaryKey)
        {
            return _pooledServices.TryGetValue(primaryKey, out ServiceBehaviour service) ? service : null;
        }
    }
}

