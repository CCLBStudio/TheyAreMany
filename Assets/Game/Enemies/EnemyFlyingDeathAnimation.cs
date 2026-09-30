using System;
using System.Threading;
using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Enemies
{
    public class EnemyFlyingDeathAnimation : MonoBehaviour, IEnemyBehaviour, IDeathAnimation, IFixedUpdate
    {
        public EnemyFacade Facade { get; set; }

        [SerializeField] private GameObject mainBody, ragdollBody;
        [SerializeField] private Collider2D collider;
        [SerializeField] private float releaseDelay = 3f;

        [Header("Projection")]
        [SerializeField] private float flyingSpeed = 12f;
        [Tooltip("Horizontal part of the projection direction (0 = straight up).")]
        [SerializeField] private float horizontalRatio = .4f;
        [Tooltip("Random angle (degrees) added to the projection direction.")]
        [SerializeField] private float directionRandomAngle = 15f;

        [Header("Spin")]
        [SerializeField] private Vector2 spinSpeedRange = new Vector2(15f, 25f);
        [Tooltip("Per-bone random extra angular velocity so limbs flail around.")]
        [SerializeField] private float limbFlailSpeed = 10f;

        [Header("Cartoon Gravity")]
        [Tooltip("Gravity multiplier applied to the ragdoll (1 = normal, < 1 = floatier).")]
        [SerializeField] private float gravityScale = 1f;

        private Rigidbody[] _ragdollBodies;
        private Transform[] _ragdollTransforms;
        private Vector3[] _initialLocalPositions;
        private Quaternion[] _initialLocalRotations;
        private Vector3[] _initialLocalScales;
        private CancellationTokenSource _releaseCts;
        private bool _ragdollActive;

        public void TriggerDeathAnimation(IDamageSource killer)
        {
            collider.enabled = false;

            EnableRagdoll();
            Project(killer);

            _releaseCts?.Cancel();
            _releaseCts?.Dispose();
            _releaseCts = new CancellationTokenSource();
            AwaitAndRelease(_releaseCts.Token);
        }

        private void Project(IDamageSource killer)
        {
            float side = killer.GetPosition().x > transform.position.x ? -1f : 1f;
            Vector3 direction = new Vector3(side * horizontalRatio, 1f, 0f).normalized;
            direction = Quaternion.AngleAxis(Random.Range(-directionRandomAngle, directionRandomAngle), Vector3.forward) * direction;
            Vector3 linearVelocity = direction * flyingSpeed;

            // Mostly spin around Z (side-view tumble), with a bit of X/Y wobble, in the direction of the projection
            float spinSpeed = Random.Range(spinSpeedRange.x, spinSpeedRange.y);
            Vector3 spinAxis = new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), -side).normalized;
            Vector3 angularVelocity = spinAxis * spinSpeed;

            Vector3 center = GetRagdollCenterOfMass();

            foreach (Rigidbody rb in _ragdollBodies)
            {
                rb.maxAngularVelocity = Mathf.Max(rb.maxAngularVelocity, spinSpeed + limbFlailSpeed);

                // Rigid-body rotation: v = v0 + ω × r, so the whole body spins as one instead of joints fighting each other
                Vector3 tangential = Vector3.Cross(angularVelocity, rb.worldCenterOfMass - center);
                rb.linearVelocity = linearVelocity + tangential;
                rb.angularVelocity = angularVelocity + Random.insideUnitSphere * limbFlailSpeed;
            }
        }

        private Vector3 GetRagdollCenterOfMass()
        {
            Vector3 sum = Vector3.zero;
            float totalMass = 0f;
            foreach (Rigidbody rb in _ragdollBodies)
            {
                sum += rb.worldCenterOfMass * rb.mass;
                totalMass += rb.mass;
            }

            return totalMass > 0f ? sum / totalMass : ragdollBody.transform.position;
        }

        public void FixedTick()
        {
            if (!_ragdollActive || Mathf.Approximately(gravityScale, 1f))
            {
                return;
            }

            Vector3 extraGravity = Physics.gravity * (gravityScale - 1f);
            foreach (Rigidbody rb in _ragdollBodies)
            {
                rb.AddForce(extraGravity, ForceMode.Acceleration);
            }
        }

        private async void AwaitAndRelease(CancellationToken token)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(releaseDelay, token);
                Facade.ReleaseSelf();
            }
            catch (OperationCanceledException)
            {
                // Enemy was released/reused before the delay ended
                Debug.LogError($"Error while waiting for enemy release : enemy was released too early");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while waiting for enemy release: {e.Message}");
            }
        }
        
        private void EnableRagdoll()
        {
            ResetRagdollPose();
            mainBody.SetActive(false);
            ragdollBody.SetActive(true);
            _ragdollActive = true;
        }

        private void DisableRagdoll()
        {
            _ragdollActive = false;

            if (ragdollBody.activeInHierarchy)
            {
                foreach (Rigidbody rb in _ragdollBodies)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            ragdollBody.SetActive(false);
            ResetRagdollPose();
            mainBody.SetActive(true);
        }

        private void ResetRagdollPose()
        {
            for (int i = 0; i < _ragdollTransforms.Length; i++)
            {
                Transform t = _ragdollTransforms[i];
                t.localPosition = _initialLocalPositions[i];
                t.localRotation = _initialLocalRotations[i];
                t.localScale = _initialLocalScales[i];
            }
        }

        public void OnEnemyCreated()
        {
            _ragdollBodies = ragdollBody.GetComponentsInChildren<Rigidbody>(true);
            _ragdollTransforms = ragdollBody.GetComponentsInChildren<Transform>(true);

            int count = _ragdollTransforms.Length;
            _initialLocalPositions = new Vector3[count];
            _initialLocalRotations = new Quaternion[count];
            _initialLocalScales = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Transform t = _ragdollTransforms[i];
                _initialLocalPositions[i] = t.localPosition;
                _initialLocalRotations[i] = t.localRotation;
                _initialLocalScales[i] = t.localScale;
            }
        }

        public void OnEnemyRequested()
        {
            collider.enabled = true;
            GlobalUpdater.RegisterFixedUpdate(this);
        }

        public void OnEnemyReleased()
        {
            _releaseCts?.Cancel();
            _releaseCts?.Dispose();
            _releaseCts = null;

            DisableRagdoll();
            GlobalUpdater.UnregisterFixedUpdate(this);
        }

        private void OnDestroy()
        {
            _releaseCts?.Cancel();
            _releaseCts?.Dispose();
        }
    }
}
