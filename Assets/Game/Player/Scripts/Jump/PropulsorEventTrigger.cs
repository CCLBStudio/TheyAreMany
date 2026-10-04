using Game.Player;
using UnityEngine;
using UnityEngine.Events;

public class PropulsorEventTrigger : MonoBehaviour, IPropulsable
{
    [SerializeField] private PlayerFacade player;
    public UnityEvent<Propulsor> onEnterPropulsorRange;
    public UnityEvent<Propulsor> onExitPropulsorRange;
    
    public void EnterPropulsorRange(Propulsor propulsor)
    {
        onEnterPropulsorRange?.Invoke(propulsor);
        //player.NotifyPropulsorEnter(propulsor);
    }

    public void ExitPropulsorRange(Propulsor propulsor)
    {
        onExitPropulsorRange?.Invoke(propulsor);
        //player.NotifyPropulsorExit(propulsor);
    }
}
