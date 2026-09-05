using UnityEngine;

public class PlayerAbilities : MonoBehaviour, IPlayerBehaviour
{
    public PlayerFacade Facade { get; set; }
    public Transform AbilityHolder => abilityHolder;

    [SerializeField] private InputReader inputReader;
    [SerializeField] private ScriptableAbility startAbility;
    [SerializeField] private Transform abilityHolder;
    [SerializeField] private Rigidbody2D playerRb;

    private RuntimeAbility _currentAbility;

    public void Initialize()
    {
        _currentAbility = startAbility.Equip(this);
        
        inputReader.PrimaryAbilityPressEvent += _currentAbility.OnInputPressed;
        inputReader.PrimaryAbilityReleaseEvent += _currentAbility.OnInputReleased;
        inputReader.AimEvent += _currentAbility.OnAim;
    }
}
