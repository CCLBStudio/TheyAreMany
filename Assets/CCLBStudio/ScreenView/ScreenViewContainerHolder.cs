using UnityEngine;

namespace CCLBStudio.ScreenView
{
    public class ScreenViewContainerHolder : MonoBehaviour
    {
        public ScreenViewContainerHandler InstantiateNewContainer(GameObject containerPrefab)
        {
            if (!containerPrefab)
            {
                Debug.LogError($"Prefab is null");
                return null;
            }
        
            var container = Instantiate(containerPrefab, transform);
            if (container.TryGetComponent(out ScreenViewContainerHandler handler))
            {
                return handler;
            }
        
            Debug.LogError($"No component {nameof(ScreenViewContainerHandler)} on {container.name}");
            Destroy(container);
            return null;
        }
    }
}
