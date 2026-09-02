using UnityEngine;
using UnityEngine.Events;

public class DamageTargetEventBinder : MonoBehaviour, IDamageTarget
{
    public UnityEvent<IDamageSource> hitEvent;
    public void ReceiveDamages(IDamageSource damageOrigin)
    {
        hitEvent?.Invoke(damageOrigin);
    }
}
