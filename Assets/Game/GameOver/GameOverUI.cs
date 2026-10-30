using System;
using CCLBStudio.DependencyInjection;
using CCLBStudio.SceneLoading;
using CCLBStudio.ScreenView;
using Game.Transition;
using UnityEngine;

namespace Game.GameOver
{
    public partial class GameOverUI : InjectedMonoBehaviour
    {
        [Inject] private TransitionService _transitionService;
        [Inject] private ScreenViewService _screenViewService;
    
        private bool _isReloadingScene;
    
        public async void ReloadScene()
        {
            try
            {
                if (_isReloadingScene)
                {
                    return;
                }
            
                _isReloadingScene = true;
                await _transitionService.FadeIn(TransitionId.VignetteFadeIn);
                _screenViewService.Close(ViewId.GameOver);
                await SceneLoader.ReloadActiveScene();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
