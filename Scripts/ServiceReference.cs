using System;
using Amazon.S3.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GeoTetra.GTPooling
{
    [Obsolete("Use ServiceReferenceT")]
    [System.Serializable]
    public class ServiceReference : AssetReferenceT<ScriptableObject>
    {
        public ServiceReference(string guid) : base(guid) { }

        internal ServiceReference(ScriptableObject service) : base(string.Empty)
        {
            _service = service;
        }

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

        internal void SetService(ScriptableObject service)
        {
            _service = service;
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
}