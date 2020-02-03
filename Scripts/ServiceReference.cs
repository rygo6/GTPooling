using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GeoTetra.GTPooling
{
    [Serializable]
    public class ServiceReference : AssetReference
    {
        protected ScriptableObject _service;

        public virtual void LoadServiceFromPool()
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
}