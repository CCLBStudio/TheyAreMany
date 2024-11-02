using UnityEngine;

public class EnemyHealth : MonoBehaviour, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private ScriptableEnemy enemyData;

    private float _currentHealth;
    
    public void OnEnemyCreated()
    {
        
    }

    public void OnEnemyRequested()
    {
        _currentHealth = enemyData.MaxHealth;
    }

    public void OnEnemyReleased()
    {
    }

    public void OnFixedUpdated()
    {
    }

    public void OnBulletHit(RuntimeBullet bullet)
    {
        _currentHealth -= bullet.GetDamages();
        if (_currentHealth <= 0f)
        {
            Facade.ReleaseSelf();
        }
    }
}