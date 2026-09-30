using CCLBStudio.ScriptablePooling;
using UnityEngine;

public class EnemyDeathParticlesPlayer : MonoBehaviour, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private ScriptablePool particlesPool;
    [SerializeField] private Transform particlesTarget;
    
    private FlyingAnimationParticles _ps;

    public void PlayParticles()
    {
        _ps = particlesPool.RequestObjectAs<FlyingAnimationParticles>();
        _ps.SetTarget(particlesTarget);
    }
    
    public void OnEnemyCreated()
    {
    }

    public void OnEnemyRequested()
    {
    }

    public void OnEnemyReleased()
    {
        _ps.SetDirty();
    }
}
