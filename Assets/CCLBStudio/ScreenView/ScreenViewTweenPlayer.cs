using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;

namespace CCLBStudio.ScreenView
{
    public class ScreenViewTweenPlayer
    {
        public event Action OnCompleted;
        public event Action OnLaunch;
        private readonly List<ScreenViewTweenDescriptor> _descriptors;
        private readonly List<Tween> _currentlyRunning;

        public ScreenViewTweenPlayer(List<ScreenViewTweenDescriptor> tweenDescriptors)
        {
            _descriptors = tweenDescriptors;
            _currentlyRunning = new List<Tween>();
            if (_descriptors.Count <= 0)
            {
                return;
            }
            
            var longestTween = _descriptors.Aggregate((max, current) => current.GetTotalTime() > max.GetTotalTime() ? current : max);
            longestTween.OnCompleted += LaunchOnCompleted;
        }

        public void Launch()
        {
            OnLaunch?.Invoke();

            if (_descriptors.Count <= 0)
            {
                LaunchOnCompleted();
                return;
            }
            
            foreach (var tween in _descriptors)
            {
                _currentlyRunning.AddRange(tween.Launch());
            }
        }

        public void KillRunningTweens()
        {
            foreach (var tween in _currentlyRunning.Where(x => x.isAlive))
            {
                tween.Stop();
            }
            
            _currentlyRunning.Clear();
        }
        
        private void LaunchOnCompleted()
        {
            OnCompleted?.Invoke();
        }
    }
}