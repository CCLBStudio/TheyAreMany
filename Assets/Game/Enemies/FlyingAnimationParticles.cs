using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FlyingAnimationParticles : MonoBehaviour, IScriptablePooledObject, IFixedUpdate
{
    public ScriptablePool Pool { get; set; }

    private Transform _target;
    private ParticleSystem _ps;
    private bool _checkForRelease;

    public void SetTarget(Transform target)
    {
        if(!target)
        {
            Debug.LogError("Target is null !");
            return;
        }
        
        _target = target;
        transform.position = _target.position;
        gameObject.SetActive(true);
    }

    public void SetDirty()
    {
        _checkForRelease = true;
    }
    
    public void OnObjectCreated()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    public void OnObjectRequested()
    {
        GlobalUpdater.RegisterFixedUpdate(this);
    }

    public void OnObjectReleased()
    {
        GlobalUpdater.UnregisterFixedUpdate(this);
        _target = null;
    }

    public void FixedTick()
    {
        if (_checkForRelease)
        {
            if (!_ps || !_ps.IsAlive())
            {
                Pool.ReleaseObject(this);
            }
            
            return;
        }
        if (!_target)
        {
            return;
        }

        transform.position = _target.position;
    }
}
