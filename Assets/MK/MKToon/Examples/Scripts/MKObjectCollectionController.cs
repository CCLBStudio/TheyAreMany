using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MK.Toon.Examples
{
    #if UNITY_6000_5_OR_NEWER
    public partial class MKObjectCollectionController : MonoBehaviour
    #else
    public class MKObjectCollectionController : MonoBehaviour
    #endif
    {
        #if UNITY_6000_5_OR_NEWER
        [Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
        #endif
        public static MKObjectCollectionController instance { get; private set; }
        private int currentIndex = 0;

        public bool animate = true;
        public float waittimeInSeconds = 3;

        [SerializeField]
        private List<GameObject> _objects = new List<GameObject>();

        private IEnumerator coroutine;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;

            if (!instance.enabled)
                return;
            currentIndex = 0;
            foreach(GameObject go in _objects)
                go.SetActive(false);
            _objects[0].SetActive(true);

            coroutine = Animate();
            if(animate)
                StartCoroutine(coroutine);
        }

        private void OnDestroy()
        {
            if(animate)
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
            _objects[currentIndex].SetActive(false);
            currentIndex++;
            if(currentIndex >= _objects.Count)
                currentIndex = 0;

            _objects[currentIndex].SetActive(true);
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
