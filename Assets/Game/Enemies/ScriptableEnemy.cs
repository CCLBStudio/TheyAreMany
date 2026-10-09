using UnityEngine;

[CreateAssetMenu(menuName = "They Are Many/Enemy/Enemy Scriptable", fileName = "NewEneyScriptable")]
public class ScriptableEnemy : ScriptableObject
{
    public int MaxHealth => maxHealth;
    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackSpeed = .5f;
}
