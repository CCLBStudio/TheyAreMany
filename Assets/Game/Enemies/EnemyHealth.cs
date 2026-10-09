using Game.Damage;
using Game.Stats;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IEnemyBehaviour, IDamageTarget
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private UnityEvent<IDamageSource> onDeath;

    private int _currentHealth;
    private ICharacterStats _stats;
    
    public void OnEnemyCreated()
    {
        if(TryGetComponent(out ICharacterStats stats))
        {
            _stats = stats;
        }
        else
        {
            Debug.LogWarning("EnemyHealth: No ICharacterStats component found. Using default stats.");
            _stats = new DefaultCharacterStats();
        }
    }

    public void OnEnemyRequested()
    {
        _currentHealth = Facade.EnemyData.MaxHealth;
    }

    public void OnEnemyReleased()
    {
    }

    public ResistanceDescriptor GetResistances(DamageType damageType)
    {
        return _stats.GetResistances(damageType);
    }

    public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
    {
        if (_currentHealth <= 0f)
        {
            return;
        }
        
        _currentHealth -= damageContext.Damage.FinalDamage;
        if (_currentHealth <= 0f)
        {
            onDeath?.Invoke(damageContext.Source);
        }
    }
}
