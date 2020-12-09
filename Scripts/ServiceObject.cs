using System;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeoTetra.GTPooling
{
    public abstract class ServiceObject : ScriptableObject
    {
        public Task Starting { get; private set; }
        
        public event Action<ServiceObject> Started;
        
        public event Action<ServiceObject> Ended;

        protected virtual Task OnServiceStart()
        {
            Started?.Invoke(this);
            return Task.CompletedTask;
        }

        protected virtual void OnServiceEnd()
        {
            Ended?.Invoke(this);
        }
 
#if UNITY_EDITOR
        protected void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayStateChange;
        }
 
        protected void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayStateChange;
        }
 
        void OnPlayStateChange(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log($"OnServiceAwake {name}");
                Starting = OnServiceStart();
            }
            else if(state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log($"OnServiceEnd {name}");
                OnServiceEnd();
            }
        }
#else
        protected void OnEnable()
        {
            OnServiceAwake();
        }
 
        protected void OnDisable()
        {
            OnServiceEnd();
        }
#endif
    }
}