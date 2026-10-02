using UnityEngine;

public interface IDamageTarget
{
    public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null);
}

public interface IHealth : IDamageTarget
{
    public void Heal(float amount);
}
