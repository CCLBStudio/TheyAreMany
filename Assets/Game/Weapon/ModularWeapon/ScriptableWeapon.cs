using System;
using Game.ModularWeapon;
using Game.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ModularWeapon", menuName = "Scriptable Objects/ModularWeapon")]
public class ScriptableWeapon : SerializedScriptableObject
{
    
    [SerializeField] private string weaponName = "new weapon";
    [SerializeField] private RuntimeWeapon weaponPrefab;
    [SerializeReference] private IWeaponModule[] modules = Array.Empty<IWeaponModule>();

    public RuntimeWeapon Equip(IWeaponOwner owner)
    {
        var weapon = Instantiate(weaponPrefab, owner.WeaponContainer);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        
        weapon.Initialize(modules, owner);
        return weapon;
    }
}
