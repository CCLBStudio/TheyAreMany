using System;
using Game.ModularAbility;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ModularAbility", menuName = "They Are Many/Abilities/Modular Ability")]
public class ScriptableAbility : SerializedScriptableObject
{
    [SerializeField] private string abilityName = "new ability";
    [SerializeField] private RuntimeAbility abilityPrefab;
    [SerializeReference] private IAbilityModule[] modules = Array.Empty<IAbilityModule>();

    public RuntimeAbility Equip(IAbilityOwner owner)
    {
        var ability = Instantiate(abilityPrefab, owner.AbilityContainer);
        ability.transform.localPosition = Vector3.zero;
        ability.transform.localRotation = Quaternion.identity;

        ability.Initialize(modules, owner);
        return ability;
    }
}