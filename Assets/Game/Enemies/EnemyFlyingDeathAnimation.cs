using UnityEngine;

namespace Game.Enemies
{
    public class EnemyFlyingDeathAnimation : MonoBehaviour, IEnemyBehaviour, IDeathAnimation
    {
        public EnemyFacade Facade { get; set; }

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D collider;
        [SerializeField] private float flyingForce = 50f, rotationSpeed = 720f, gravityScale = 2, linearDamping = 1;

        private bool _triggered;
        private float _initialGravity, _initialLinearDamping;
        
        public void Trigger()
        {
            collider.enabled = false;
            rb.gravityScale = gravityScale;
            rb.linearDamping = linearDamping;
            _triggered = true;
            rb.AddForce(new Vector2(.4f, 1f).normalized * flyingForce, ForceMode2D.Impulse);
            Invoke(nameof(Release), 5f);
        }

        private void Release()
        {
            Facade.ReleaseSelf();
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

        public void OnFixedUpdated()
        {
            if (!_triggered)
            {
                return;
            }
            
            rb.MoveRotation(rb.rotation + rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
