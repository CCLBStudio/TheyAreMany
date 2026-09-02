using System;
using System.Threading.Tasks;
using CCLBStudio.Attributes;
using CCLBStudio.DependencyInjection;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public class ScreenViewSoundFeedback : IScreenViewFeedback
    {
        [NullInfo("Will create a default one")]
        [SerializeField] private GameObject audioSource;
        [NullInfo("Will use service default clip")]
        [SerializeField] private AudioClip clip;
        [SerializeField] private float volume = 1f;

        [Inject] private ScreenViewService _service;
        [NonSerialized] private bool _injected;
        
        public void PlayFeedback()
        {
            if (!clip)
            {
                if(!_injected)
                {
                    _injected = true;
                    Injector.InjectNewConsumer(this);
                }

                clip = _service.DefaultClickSound;
            }
            
            PlayClipWithCallback();
        }

        private async void PlayClipWithCallback()
        {
            if (!clip)
            {
                Debug.LogError("Clip is null !");
                return;
            }
            
            var source = BindSource();
            
            source.clip = clip;
            source.volume = volume;
            source.Play();
            
            float duration = clip.length / source.pitch;
            await Task.Delay(Mathf.CeilToInt(duration * 1000f));
            
            UnityEngine.Object.Destroy(source.gameObject);
        }

        private AudioSource BindSource()
        {
            if (!audioSource || !audioSource.TryGetComponent(out AudioSource _))
            {
                return new GameObject("AudioSource").AddComponent<AudioSource>();
            }

            return UnityEngine.Object.Instantiate(audioSource).GetComponent<AudioSource>();
        }
    }
}
