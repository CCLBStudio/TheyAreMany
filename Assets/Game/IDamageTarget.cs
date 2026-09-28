using UnityEngine;

public interface IDamageTarget
{
    public void ReceiveDamages(IDamageSource damageSource);
}

public interface IHealth : IDamageTarget
{
    public void Heal(float amount);
}
