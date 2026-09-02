using System;
using System.Collections.Generic;
using System.Linq;

namespace CCLBStudio.ScreenView
{
    public class ScreenViewAnimator
    {
        public event Action OnCompleted;
        public event Action OnLaunch;
        
        protected List<IScreenViewAnimation> animations;
        
        public void Initialize(IEnumerable<IScreenViewAnimation> anims)
        {
            animations = new List<IScreenViewAnimation>(anims);
            if (animations.Count <= 0)
            {
                return;
            }
            
            var longestAnim = animations.Aggregate((max, current) => current.GetTotalTime() > max.GetTotalTime() ? current : max);
            longestAnim.OnCompleted += RaiseOnCompleted;
        }

        public void Launch()
        {
            RaiseOnLaunch();
            
            if (animations.Count <= 0)
            {
                RaiseOnCompleted();
                return;
            }

            foreach (var anim in animations)
            {
                anim.Animate();
            }
        }

        public void Kill()
        {
            foreach (var anim in animations)
            {
                anim.Kill();
            }
        }

        private void RaiseOnLaunch()
        {
            OnLaunch?.Invoke();
        }

        private void RaiseOnCompleted()
        {
            OnCompleted?.Invoke();
        }
    }
}


