using System;
using CCLBStudio.GlobalUpdater;
using UnityEngine;

namespace Game.Enemies
{
    public class EnemyFlyingDeathAnimation : MonoBehaviour, IFixedUpdate, IEnemyBehaviour, IDeathAnimation
    {
        public EnemyFacade Facade { get; set; }

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D collider;
        [SerializeField] private float flyingForce = 50f, rotationSpeed = 720f, gravityScale = 2, linearDamping = 1;

        private bool _triggered;
        private float _initialGravity, _initialLinearDamping;
        
        public void TriggerDeathAnimation(IDamageSource killer)
        {
            collider.enabled = false;
            rb.gravityScale = gravityScale;
            rb.linearDamping = linearDamping;
            _triggered = true;
            float x = killer.GetPosition().x > transform.position.x ? -1f : 1f;
            rb.AddForce(new Vector2(x * .4f, 1f).normalized * flyingForce, ForceMode2D.Impulse);
            AwaitAndRelease();
        }

        private async void AwaitAndRelease()
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(3f);
                Facade.ReleaseSelf();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while waiting for enemy release: {e.Message}");
            }

        }

        public void OnEnemyCreated()
        {
            _initialGravity = rb.gravityScale;
            _initialLinearDamping = rb.linearDamping;
        }

        public void OnEnemyRequested()
        {
            collider.enabled = true;
            _triggered = false;
            rb.gravityScale = _initialGravity;
            rb.linearDamping = _initialLinearDamping;
        }

        public void OnEnemyReleased()
        {

        }

        public void FixedTick()
        {
            if (!_triggered)
            {
                return;
            }
            
            rb.MoveRotation(rb.rotation + rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
