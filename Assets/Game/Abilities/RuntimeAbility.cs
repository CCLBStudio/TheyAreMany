using Sirenix.OdinInspector;
using UnityEngine;

public abstract class RuntimeAbility : MonoBehaviour
{
    public abstract void Initialize(PlayerAbilities playerAbilities, ScriptableAbility rocketAbility);
    public abstract void OnAim(Vector2 direction);
    public abstract void OnInputPressed();
    public abstract void OnInputReleased();

    protected virtual Quaternion ComputeRotationTowardAxis(Vector3 direction)
    {
        return Quaternion.FromToRotation(Vector3.right, direction);
    }
}
