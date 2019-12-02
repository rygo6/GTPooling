using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GeoTetra.GTPooling
{
    [Serializable]
    public class ServiceReference : AssetReference
    {
        private ScriptableObject _service;

        public void LoadServiceFromPool()
        {
            AddressableServicesPool.GlobalPool.PrePooledPopulate(this, out _service);
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
}