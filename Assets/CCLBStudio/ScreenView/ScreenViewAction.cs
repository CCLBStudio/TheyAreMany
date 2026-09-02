using System;
using CCLBStudio.DependencyInjection;
using Systems.ScriptableBehaviours;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [CreateAssetMenu(menuName = "CCLB Studio/Screen View/Scriptable Actions/Screen View Action", fileName = "NewScreenViewAction")]
    public class ScreenViewAction : ScriptableAction
    {
        [SerializeField] private ViewId viewId;
        [SerializeField] private ViewAction action = ViewAction.Show;
        
        [NonSerialized] [Inject]
        private ScreenViewService _screenViewService;
        
        private enum ViewAction {Show, Close}
        
        public override void Execute()
        {
            if (!_screenViewService)
            {
                Injector.InjectNewConsumer(this);
            }
            
            switch(action)
            {
                case ViewAction.Show:
                    _screenViewService.Show(viewId);
                    break;
                
                case ViewAction.Close:
                    _screenViewService.Close(viewId);
                    break;
            }
        }
    }
}
