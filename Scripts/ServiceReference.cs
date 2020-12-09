using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GeoTetra.GTPooling
{
    [System.Serializable]
    public class ServiceObjectReferenceT<ServiceObjectType> : AssetReferenceT<ServiceObjectType> where ServiceObjectType : ServiceObject
    {
        public ServiceObjectReferenceT(string guid) : base(guid) { }

        private ServiceObjectType _ref;
        private AsyncOperationHandle<ServiceObjectType> _handle;
        private SubscribableBehaviour _subscribableBehaviour;
        private ServiceObject _subscribableServiceObject;
        
        public static implicit operator ServiceObjectType(ServiceObjectReferenceT<ServiceObjectType> referenceT) => referenceT.Ref;
        public static implicit operator bool(ServiceObjectReferenceT<ServiceObjectType> referenceT) => referenceT.Ref != null;
        
        public ServiceObjectType Ref
        {
            get
            {
                if (_ref == null)
                {
                    Debug.Log($"Service not cached {typeof(ServiceObjectType).Name} call 'await .Cache()'.");
                }
                return _ref;
            }
            internal set => _ref = value;
        }
        
        /// <summary>
        /// Loads the reference for future Service calls.
        /// </summary>
        public async Task Cache(SubscribableBehaviour subscribableBehaviour)
        {
            if (_ref == null)
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
            if (_ref == null)
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
            if (_ref != null && _ref.Starting != null) await _ref.Starting;
            _ref = null;
            
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
                _handle = Addressables.LoadAssetAsync<ServiceObjectType>(typeof(ServiceObjectType).Name);
                _ref = await _handle.Task;
            }
            else
            {
                _handle = LoadAssetAsync<ServiceObjectType>();
                _ref = await _handle.Task;
            }
            
            _ref.StartService();
            await _ref.Starting;
        }
    }
}