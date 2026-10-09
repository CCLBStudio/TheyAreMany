using Game.Damage;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IEnemyBehaviour, IDamageTarget
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private UnityEvent<IDamageSource> onDeath;

    private int _currentHealth;
    
    public void OnEnemyCreated()
    {
        
    }

    public void OnEnemyRequested()
    {
        _currentHealth = Facade.EnemyData.MaxHealth;
    }

    public void OnEnemyReleased()
    {
    }

    public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
    {
        if (_currentHealth <= 0f)
        {
            return;
        }
        
        _currentHealth -= damageContext.Amount();
        if (_currentHealth <= 0f)
        {
            onDeath?.Invoke(damageContext.Source());
        }
    }
}
