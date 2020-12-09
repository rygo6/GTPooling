using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
#if UNITY_EDITOR

#endif

namespace GeoTetra.GTPooling
{
    [System.Serializable]
    public class ComponentContainerRef : ServiceObjectReferenceT<ComponentContainer>
    {
        public ComponentContainerRef(string guid) : base(guid)
        { }
        
        private Dictionary<Type, object> _awaitingRegister;

        public async Task CacheAndRegister(SubscribableBehaviour subscribableBehaviour)
        {
            await Cache(subscribableBehaviour);
            Ref.RegisterComponent(subscribableBehaviour);
        }

        /// <summary>
        /// Allows you to wait till a component is registered. This logic should occur here,
        /// rather than in ComponentContainer, so in case the object which created this ref disappears
        /// it will also destroy this data waiting for components.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> AwaitRegister<T>() where T : SubscribableBehaviour
        {
            T result = Ref.Get<T>();
            if (result != null)
            {
                return result;
            }
                
            if (_awaitingRegister == null)
            {
                _awaitingRegister = new Dictionary<Type, object>();
                Ref.ComponentRegistered += CheckComponentRegistered;
            }
            
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            _awaitingRegister.Add(typeof(T), tcs);
            result = await tcs.Task;
            return result;
        }

        private void CheckComponentRegistered(SubscribableBehaviour subscribableBehaviour)
        {
            _awaitingRegister.TryGetValue(subscribableBehaviour.GetType(), out object tcsObject);
            if (tcsObject != null)
            {
                Type tcsType = typeof(TaskCompletionSource<>).MakeGenericType(subscribableBehaviour.GetType());
                MethodInfo methodInfo = tcsType.GetMethod("SetResult");
                methodInfo?.Invoke(tcsObject, new object[] { subscribableBehaviour });
                _awaitingRegister.Remove(subscribableBehaviour.GetType());
            }
        }

        private void CheckRegisteredComponent<T>(SubscribableBehaviour subscribableBehaviour, TaskCompletionSource<T> tcs) where T : SubscribableBehaviour
        {
            if (subscribableBehaviour is T behaviour)
            {
                tcs.SetResult(behaviour);
            }
        }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/ComponentContainer")]
    public class ComponentContainer : ServiceObject
    {
        public event Action<SubscribableBehaviour> ComponentRegistered;
        
        private readonly Dictionary<Type, SubscribableBehaviour> _objectDictionary = new Dictionary<Type, SubscribableBehaviour>();

        protected override async Task OnServiceStart()
        {
            Clear();
            await base.OnServiceStart();
        }
        

        private void Clear()
        {
            _objectDictionary.Clear();
        }

        public void Populate<T>(out T reference) where T : SubscribableBehaviour
        {
            reference = Get<T>();
        }
        
        public T Get<T>() where T : SubscribableBehaviour
        {
            _objectDictionary.TryGetValue(typeof(T), out SubscribableBehaviour returnObject);
            return (T)returnObject;
        }

        public void RegisterComponent(SubscribableBehaviour behaviour)
        {
            Debug.Log($"Registering {behaviour} in ComponentContainer {this}.");
            _objectDictionary.Add(behaviour.GetType(), behaviour);
            behaviour.Destroyed += UnregisterComponent;
            ComponentRegistered?.Invoke(behaviour);
        }

        public void UnregisterComponent(SubscribableBehaviour unregisterBehaviour)
        {
            Debug.Log($"Unegistering {unregisterBehaviour} from ComponentContainer {this}.");
            _objectDictionary.Remove(unregisterBehaviour.GetType());
        }
    }
}