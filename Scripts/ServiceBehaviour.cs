using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GeoTetra.GTPooling
{
    public class ServiceBehaviour : MonoBehaviour
    {
        public bool IsLoaded { get; protected set; }

        public event Action<ServiceBehaviour> LoadCompleted;

        private int _waitingToLoadDepdencyCount;

        public void OnLoadComplete()
        {
            IsLoaded = true;
            LoadCompleted?.Invoke(this);
        }

        protected void RegisterDependencies(params AssetReference[] serviceReferences)
        {
            // foreach (AssetReference serviceReference in serviceReferences)
            // {
            //     if (!serviceReference.IsDone)
            //     {
            //         _waitingToLoadDepdencyCount++;
            //         service.LoadCompleted += DependentServiceOnLoadCompleted;
            //     }
            // }
        }

        private void DependentServiceOnLoadCompleted(ServiceBehaviour obj)
        {
            _waitingToLoadDepdencyCount--;
            if (_waitingToLoadDepdencyCount == 0)
            {
                OnLoadComplete();
            }
        }
    }
}