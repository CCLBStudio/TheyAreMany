using UnityEngine;

public interface IDamageTarget
{
    public void ReceiveDamages(IDamageSource damageSource);
}
