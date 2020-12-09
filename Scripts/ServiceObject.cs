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
            Debug.Log($"OnServiceStart {name}");
            Started?.Invoke(this);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to implement service cleanup. Settings of service will
        /// not clear between 'Plays' in Unity when Addressable is set to 'Use Asset Database'.
        /// </summary>
        protected virtual void OnServiceEnd()
        {
            Debug.Log($"OnServiceEnd {name}");
            Ended?.Invoke(this);
        }

        internal void StartService()
        {
            if (Starting == null) Starting = OnServiceStart();
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
            // To play mode
            if(state == PlayModeStateChange.ExitingEditMode)
            {
                Starting = null;
                // Debug.Log($"OnServiceAwake {name}");
                // Starting = OnServiceStart();
            }
            // To editor mode
            else if(state == PlayModeStateChange.ExitingPlayMode && Starting != null)
            {
                OnServiceEnd();
            }
        }
#else
        protected void OnEnable()
        {
            // OnServiceAwake();
        }
 
        protected void OnDisable()
        {
            OnServiceEnd();
        }
#endif
    }
}