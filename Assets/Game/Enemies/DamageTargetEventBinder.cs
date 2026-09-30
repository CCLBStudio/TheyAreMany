using UnityEngine;
using UnityEngine.Events;

public class DamageTargetEventBinder : MonoBehaviour, IDamageTarget
{
    public UnityEvent<IDamageSource, Vector3?> hitEvent;
    public void ReceiveDamages(IDamageSource damageOrigin, Vector3? hitPoint = null)
    {
        hitEvent?.Invoke(damageOrigin, hitPoint);
    }
}
