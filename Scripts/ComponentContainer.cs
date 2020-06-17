using System.Collections.Generic;
using GeoTetra.GTCommon.Components;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeoTetra.GTPooling
{
    [System.Serializable]
    public class ComponentContainerReference : ServiceReferenceT<ComponentContainer>
    {
        public ComponentContainerReference(string guid) : base(guid)
        { }
    }
    
    public class ComponentContainer : ServiceBehaviour
    {
#if UNITY_EDITOR
        [Multiline] 
        [SerializeField]
        private string _developerDescription;
#endif

        private readonly Dictionary<System.Type, Object> _objectDictionary = new Dictionary<System.Type, Object>();

        private void OnEnable()
        {
            Clear();
        }

        private void Clear()
        {
            _objectDictionary.Clear();
        }

        public void Populate<T>(out T reference) where T : Object
        {
            reference = Get<T>();
        }
        
        public T Get<T>() where T : Object
        {
            _objectDictionary.TryGetValue(typeof(T), out Object returnObject);
            return (T)returnObject;
        }

        public void RegisterComponent(SubscribableBehaviour behaviour)
        {
            _objectDictionary.Add(behaviour.GetType(), behaviour);
            behaviour.Destroyed += UnregisterComponent;
        }

        public void UnregisterComponent(Object unregisterObject)
        {
            Debug.Log("Unregistering " + unregisterObject);
            _objectDictionary.Remove(unregisterObject.GetType());
        }
    }
}