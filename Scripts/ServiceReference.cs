using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GeoTetra.GTPooling
{
    [System.Serializable]
    public class ServiceReference : AssetReferenceT<ScriptableObject>
    {
        public ServiceReference(string guid) : base(guid) { }

        private ScriptableObject _service;

        protected virtual void LoadServiceFromPool()
        {
            _service = AddressableServicesPool.GlobalPool.PrePooledPopulate<ScriptableObject>(this);
            if (_service == null)
            {
                Debug.LogWarning($"{this.ToString()} Cannot find reference");
            }
        }
        
        public T Service<T>() where T : ScriptableObject
        {
            if (_service == null) LoadServiceFromPool();
            return (T) _service;
        }
    }
    
    [System.Serializable]
    public class ServiceReferenceT<ServiceType> : AssetReferenceT<ServiceType> where ServiceType : ScriptableObject
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
        }

        private void LoadServiceFromPool()
        {
            //If the Asset is not explicitly set, then it will try to load one that is named the same as the type.
            _service = string.IsNullOrEmpty(AssetGUID) ? 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceType>() : 
                AddressableServicesPool.GlobalPool.PrePooledPopulate<ServiceType>(this);
        }
    }
}