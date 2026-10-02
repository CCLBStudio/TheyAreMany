using UnityEngine;
using UnityEngine.Events;

public class DamageTargetEventBinder : MonoBehaviour, IDamageTarget
{
    public UnityEvent<IDamageContext, Vector3?> hitEvent;
    public void ReceiveDamages(IDamageContext damageOrigin, Vector3? hitPoint = null)
    {
        hitEvent?.Invoke(damageOrigin, hitPoint);
    }
}
