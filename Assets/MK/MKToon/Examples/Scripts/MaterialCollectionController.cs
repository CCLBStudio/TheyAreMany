using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MK.Toon.Examples
{
    #if UNITY_6000_5_OR_NEWER
    public partial class MaterialCollectionController : MonoBehaviour
    #else
    public class MaterialCollectionController : MonoBehaviour
    #endif
    {
        #if UNITY_6000_5_OR_NEWER
        [Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
        #endif
        public static MaterialCollectionController instance { get; private set; }
        public List<Material> skyboxes = new List<Material>();
        private int currentIndex = 0;

        public bool animateMaterials = true;
        public float waittimeInSeconds = 3;

        public UnityAction changeMaterialSets = () => { };

        private IEnumerator coroutine;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
            changeMaterialSets = () => { };
            currentIndex = 0;
            coroutine = Animate();
            if(animateMaterials)
                StartCoroutine(coroutine);

            if(skyboxes.Count > 0)
            {
                RenderSettings.skybox = skyboxes[currentIndex];
            }
        }

        private void OnDestroy()
        {
            if(animateMaterials)
                StopCoroutine(coroutine);
        }

        IEnumerator Animate()
        {
            yield return new WaitForSeconds(waittimeInSeconds);
            ChangeAll();

            StopCoroutine(coroutine);
            coroutine = Animate();
            StartCoroutine(coroutine);
        }

        private void ChangeAll()
        {
            currentIndex++;
            if(currentIndex >= skyboxes.Count)
                currentIndex = 0;

            changeMaterialSets();
            if(skyboxes.Count > 0)
            {
                RenderSettings.skybox = skyboxes[currentIndex];
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                ChangeAll();
            }
        }
    }
}
