using Game.ModularAbility;
using Game.Player;
using Game.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour, IPlayerBehaviour, IAbilityOwner
{
    public PlayerFacade Facade { get; set; }
    public Transform AbilityContainer => abilityHolder;
    // Abilities currently ignore the player's stats, matching previous rocket behaviour.
    public ICharacterStats Stats { get; } = new DefaultCharacterStats();

    [SerializeField] private InputReader inputReader;
    [SerializeField] private ScriptableAbility startAbility;
    [SerializeField] private Transform abilityHolder;
    [SerializeField] private Rigidbody2D playerRb;

    private RuntimeAbility _currentAbility;

    public void Initialize()
    {
        _currentAbility = startAbility.Equip(this);
        
        inputReader.PrimaryAbilityPressEvent += _currentAbility.PressInput;
        inputReader.PrimaryAbilityReleaseEvent += _currentAbility.ReleaseInput;
        inputReader.AimEvent += _currentAbility.Aim;
    }

    #region Editor
    #if UNITY_EDITOR

    [Button]
    private void LaunchAbility()
    {
        if (_currentAbility)
        {
            _currentAbility.PressInput();
        }
    }
    
    #endif
    #endregion
}