using CCLBStudio.ScriptablePooling;
using UnityEngine;

public class EnemyDeathParticlesPlayer : MonoBehaviour, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private ScriptablePool particlesPool;
    
    private DefaultPooledObject _ps;

    public void PlayParticles()
    {
        _ps = particlesPool.RequestObjectAs<DefaultPooledObject>();
        _ps.transform.parent = transform;
        _ps.transform.localPosition = Vector3.zero;
    }
    
    public void OnEnemyCreated()
    {
    }

    public void OnEnemyRequested()
    {
    }

    public void OnEnemyReleased()
    {
        particlesPool.ReleaseObject(_ps);
    }
}
