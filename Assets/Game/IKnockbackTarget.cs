using UnityEngine;

public interface IKnockbackTarget
{
    public void ApplyKnockback(Vector3 direction, Vector3 inAirModifier);
}
