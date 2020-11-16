using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeoTetra.GTPooling
{
    public abstract class ServiceObject : ScriptableObject
    {
        protected abstract Task OnServiceAwake();
        protected abstract void OnServiceEnd();
 
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
                OnServiceAwake();
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