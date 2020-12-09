using System;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GeoTetra.GTPooling
{
    [Obsolete("Use ServiceReferenceT")]
    [System.Serializable]
    public class ServiceReference : AssetReferenceT<GameObject>
    {
        public ServiceReference(string guid) : base(guid) { }

        internal ServiceReference(ServiceBehaviour service) : base(string.Empty)
        {
            _service = service;
        }

        private ServiceBehaviour _service;

        protected virtual void LoadServiceFromPool()
        {
            _service = string.IsNullOrEmpty(AssetGUID) ? 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceBehaviour>() : 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceBehaviour>(this);
            if (_service == null)
            {
                Debug.LogWarning($"{this.ToString()} Cannot find reference");
            }
        }

        public T Service<T>() where T : ServiceBehaviour
        {
            if (_service == null) LoadServiceFromPool();
            return (T) _service;
        }

        internal void SetService(ServiceBehaviour service)
        {
            _service = service;
        }
    }
    
    [System.Serializable]
    public class ServiceReferenceT<ServiceType> : AssetReferenceT<GameObject> where ServiceType : ServiceBehaviour
    {
        public ServiceReferenceT(string guid) : base(guid) { }

        private ServiceType _service;

        public ServiceType Service
        {
            get
            {
                if (_service == null) LoadServiceFromPool();
                return _service;
            }
            internal set => _service = value;
        }

        private void LoadServiceFromPool()
        {
            //If the Asset is not explicitly set, then it will try to load one that is named the same as the type.
            _service = string.IsNullOrEmpty(AssetGUID) ? 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceType>() : 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceType>(this);
        }
    }
    
    [System.Serializable]
    public class ServiceObjectReferenceT<ServiceObjectType> : AssetReferenceT<ServiceObjectType> where ServiceObjectType : ServiceObject
    {
        public ServiceObjectReferenceT(string guid) : base(guid) { }

        private ServiceObjectType _service;
        private AsyncOperationHandle<ServiceObjectType> _handle;
        private SubscribableBehaviour _subscribableBehaviour;
        private ServiceObject _subscribableServiceObject;
        
        public static implicit operator ServiceObjectType(ServiceObjectReferenceT<ServiceObjectType> referenceT) => referenceT.Ref;
        public static implicit operator bool(ServiceObjectReferenceT<ServiceObjectType> referenceT) => referenceT.Ref != null;
        
        public ServiceObjectType Ref
        {
            get
            {
                if (_service == null)
                {
                    Debug.Log($"Service not cached {typeof(ServiceObjectType).Name} call 'await .Cache()'.");
                }
                return _service;
            }
            internal set => _service = value;
        }
        
        /// <summary>
        /// Loads the reference for future Service calls.
        /// </summary>
        public async Task Cache(SubscribableBehaviour subscribableBehaviour)
        {
            if (_service == null)
            {
                _subscribableBehaviour = subscribableBehaviour;
                _subscribableBehaviour.Destroyed += SubscribableBehaviourOnDestroyed;
                await LoadService();
            }
        }
        
        /// <summary>
        /// Loads the reference for future Service calls.
        /// </summary>
        public async Task Cache(ServiceObject serviceObject)
        {
            if (_service == null)
            {
                _subscribableServiceObject = serviceObject;
                _subscribableServiceObject.Ended += SubscribableServiceObjectOnEnded;
                await LoadService();
            }
        }

        private void SubscribableServiceObjectOnEnded(ServiceObject serviceObject)
        {
            _subscribableServiceObject.Ended -= SubscribableServiceObjectOnEnded;
            _subscribableServiceObject = null;
            Release();
        }

        private void SubscribableBehaviourOnDestroyed(SubscribableBehaviour subscribableBehaviour)
        {
            _subscribableBehaviour.Destroyed -= SubscribableBehaviourOnDestroyed;
            _subscribableBehaviour = null;
            Release();
        }

        private async void Release()
        {
            // In case object load call was sent, immediately followed by release call.
            await _handle.Task;
            await _service.Starting;
            
            if (OperationHandle.IsValid())
            {
                ReleaseAsset();
            }
            else
            {
                Addressables.Release(_handle);
            }
        }

        private async Task LoadService()
        {
            if (string.IsNullOrEmpty(AssetGUID))
            {
                IResourceLocation location = AddressablesPoolUtility.GetResourceLocation<ServiceObjectType>(typeof(ServiceObjectType).Name);
                if (location != null)
                {
                    // Debug.Log("No ServiceObjectReference specified, loading default service by name of type for " + typeof(ServiceObjectType).Name);
                }
                else
                {
                    Debug.LogWarning("No ServiceObjectReference specified, and could not find a default service by name of type "  + typeof(ServiceObjectType).Name);
                }
                
                _handle = Addressables.LoadAssetAsync<ServiceObjectType>(location.PrimaryKey);
                _service = await _handle.Task;
            }
            else
            {
                _handle = LoadAssetAsync<ServiceObjectType>();
                _service = await _handle.Task;
            }

            await _service.Starting;
        }
    }
}