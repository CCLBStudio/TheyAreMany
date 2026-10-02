using CCLBStudio.ScriptablePooling;
using UnityEngine;

public abstract class ScriptableAbility : ScriptableObject
{
    public float Cooldown => cooldown;
    public int Strength => strength.GetRandom();
    public RuntimeAbility AbilityPrefab => abilityPrefab;
    public ScriptablePool VisualPool => visualPool;

    [Header("Stats")]
    [SerializeField][Min(0f)] private float cooldown;
    [SerializeField] private RangeInt strength = new(80, 100);

    [Header("Visuals")]
    [SerializeField] private RuntimeAbility abilityPrefab;
    [SerializeField] private ScriptablePool visualPool;

    public virtual RuntimeAbility Equip(PlayerAbilities playerAbilities)
    {
        var ability = Instantiate(abilityPrefab.gameObject, playerAbilities.AbilityHolder).GetComponent<RuntimeAbility>();
        
        visualPool.Initialize();
        ability.Initialize(playerAbilities, this);
        
        return ability;
    }
}
