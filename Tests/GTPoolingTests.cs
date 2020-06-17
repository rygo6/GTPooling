using System.Collections;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ComponentContainerTests
{
    [UnityTest]
    public IEnumerator RegisterComponentAndGet()
    {
        // SubscribableBehaviour subscribableBehaviour = CreateSubscribableBehaviour();
        // ComponentContainer componentContainer = ScriptableObject.CreateInstance<ComponentContainer>();
        // componentContainer.RegisterComponent(subscribableBehaviour);
        // Assert.True(componentContainer.Get<SubscribableBehaviour>() != null);
        // MonoBehaviour.Destroy(subscribableBehaviour.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RegisterComponentAndDestroyComponent()
    {
        // SubscribableBehaviour subscribableBehaviour = CreateSubscribableBehaviour();
        // ComponentContainer componentContainer = ScriptableObject.CreateInstance<ComponentContainer>();
        // componentContainer.RegisterComponent(subscribableBehaviour);
        // MonoBehaviour.Destroy(subscribableBehaviour.gameObject);
        yield return null;
        // Assert.True(componentContainer.Get<SubscribableBehaviour>() == null);
    }

    private SubscribableBehaviour CreateSubscribableBehaviour()
    {
        GameObject testGameObject = new GameObject("TestGameObject");
        return testGameObject.AddComponent<SubscribableBehaviour>();
    }
}