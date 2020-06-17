using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GeoTetra.GTPooling
{
    public class AddressablesPool : MonoBehaviour
    {
        [SerializeField] private int _retainedInstanceLimit = 32;
        
        /// <summary>
        /// Stores released objects by primary key to be quickly looked up.
        /// </summary>
        private Dictionary<string, List<PoolObject>>  _releasedObjects = new Dictionary<string, List<PoolObject>>();
        
        /// <summary>
        /// Stores released objects in a list sorted by most recently released. So then the last released can be found.
        /// </summary>
        private List<PoolObject> _releasedObjectList = new List<PoolObject>();
        
        /// <summary>
        /// Objects currently in use.
        /// </summary>
        private Dictionary<GameObject, PoolObject>  _usedObjects = new Dictionary<GameObject, PoolObject>();

        private class PoolObject
        {
            public readonly AsyncOperationHandle<GameObject> Handle;
            public readonly IResourceLocation Location;
            public Component ComponentReference;

            public PoolObject(AsyncOperationHandle<GameObject> handle, 
                IResourceLocation location,
                Component componentReference)
            {
                Handle = handle;
                Location = location;
                ComponentReference = componentReference;
            }
        }
            
        public async Task<GameObject> PoolInstantiateAsync(AssetReference reference)
        {
            var handle = await InstantiateAsync(reference.RuntimeKey);
            return handle.Handle.Result;
        }
        
        public async Task<T> PoolInstantiateAsync<T>(AssetReference reference) where T : Component
        {
            PoolObject handle = await InstantiateAsync(reference);
            if (handle.ComponentReference == null) handle.ComponentReference = handle.Handle.Result.GetComponent<T>();
            return (T)handle.ComponentReference;
        }

        public async Task<GameObject> PoolInstantiateAsync(object key)
        {
            var handle = await InstantiateAsync(key);
            return handle.Handle.Result;
        }

        private async Task<PoolObject> InstantiateAsync(object key)
        {
            IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<GameObject>(key);
            PoolObject poolObject;
            
            //If location is initially null, means the internal ResourceManager has not alloced, so instantiate
            //by string key to trigger that.
            if (location == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, trackHandle: false);
                await handle.Task;
                location = AddressablesPoolUtility.GetResourceLocation<GameObject>(key);
                poolObject = AddNewUsedObject(handle.Result, handle, location);
                return poolObject;
            }
            
            poolObject = RetrieveObject(location.PrimaryKey);
            if (poolObject == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(location, trackHandle: false);
                await handle.Task;
                poolObject = AddNewUsedObject(handle.Result, handle, location);
                return poolObject;
            }
            else
            {
                _usedObjects.Add(poolObject.Handle.Result, poolObject);
                return poolObject;
            }
        }

        public void ReleaseToPool(GameObject instance)
        {
            if (_usedObjects.TryGetValue(instance, out PoolObject poolObject))
            {
                instance.transform.SetParent(transform);
                _usedObjects.Remove(instance);
                AddToReleasedObjects(poolObject);
            }
            else
            {
                Debug.LogWarning($"{instance} not found to release.");
            }
        }

        private PoolObject AddNewUsedObject(GameObject instance, AsyncOperationHandle<GameObject> handle, IResourceLocation location)
        {
            PoolObject poolObject = new PoolObject(handle, location, null);
            _usedObjects.Add(instance, poolObject);
            return poolObject;
        }
        
        private PoolObject RetrieveObject(string primaryKey)
        {
            _releasedObjects.TryGetValue(primaryKey, out List<PoolObject> poolList);
            if (poolList == null || poolList.Count == 0)
            {
                return null;
            }

            PoolObject poolObject = poolList[0];
            poolObject.Handle.Result.transform.SetParent(null);
            poolList.RemoveAt(0);
            return poolObject;
        }

        private void AddToReleasedObjects(PoolObject poolObject)
        {
            if (!_releasedObjects.TryGetValue(poolObject.Location.PrimaryKey, out List<PoolObject> addPoolList))
            {
                addPoolList = new List<PoolObject>();
                _releasedObjects.Add(poolObject.Location.PrimaryKey, addPoolList);
            }

            addPoolList.Add(poolObject);
            _releasedObjectList.Add(poolObject);
            
            //if limit of retained instances reached, release oldest
            if (_releasedObjectList.Count > _retainedInstanceLimit)
            {
                PoolObject releasingPoolObject = _releasedObjectList[0];
                _releasedObjectList.RemoveAt(0);
                
                if (_releasedObjects.TryGetValue(releasingPoolObject.Location.PrimaryKey, out List<PoolObject> releasingPoolList))
                {
                    releasingPoolList.Remove(releasingPoolObject);
                }
            }
        }
    }
}

