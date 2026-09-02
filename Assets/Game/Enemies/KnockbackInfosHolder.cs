using UnityEngine;

public class KnockbackInfosHolder : MonoBehaviour, IKnockbackTarget
{
    [SerializeField] private Rigidbody2D rb;

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }
}
