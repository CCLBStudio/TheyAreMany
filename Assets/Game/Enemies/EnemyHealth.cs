using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IEnemyBehaviour, IDamageTarget
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private UnityEvent onDeath;

    private float _currentHealth;
    
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

    public void OnFixedUpdated()
    {
    }

    public void ReceiveDamages(IDamageSource damageSource)
    {
        if (_currentHealth <= 0f)
        {
            return;
        }
        
        _currentHealth -= damageSource.GetDamages();
        if (_currentHealth <= 0f)
        {
            onDeath?.Invoke();
        }
    }
}
