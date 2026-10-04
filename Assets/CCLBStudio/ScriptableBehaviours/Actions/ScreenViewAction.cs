using CCLBStudio.DependencyInjection;
using CCLBStudio.ScreenView;
using Systems.ScriptableBehaviours;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScreenViewAction", menuName = "CCLBStudio/Scriptable Behaviour/Actions/Show ScreenView Action")]
public partial class ScreenViewAction : ScriptableAction
{
    [SerializeField] private ViewId id;
    [SerializeField] private ViewAction action;

    //[Inject] private ScreenViewService _service;
    
    private enum ViewAction {Show, Close}
    
    public override void Execute()
    {
        if (action == ViewAction.Show)
        {
            //_service.Show(id);
        }
        else
        {
            //_service.Close(id);
        }
    }
}
