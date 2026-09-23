using UnityEngine;

public class MeleeWeaponDamageDealer : MonoBehaviour, IDamageSource
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageTarget t))
        {
            Debug.Log($"Inflict damages to {other.name}");
            t.ReceiveDamages(this);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Slash;
    }

    public float GetDamages()
    {
        return 10f;
    }
}
