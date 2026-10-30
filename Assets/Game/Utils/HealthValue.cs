using CCLBStudio.ScriptableValue;
using UnityEngine;

namespace Game.Utils
{
    [CreateAssetMenu(fileName = "NewHealthValue", menuName = "They Are Many/Values/HealthValue")]
    public class HealthValue : ScriptableValue<float>
    {
#if UNITY_EDITOR
        public static string MaxValueProperty => nameof(maxValue);
#endif

        public float MaxValue => maxValue;
        public float NormalizedValue => maxValue > 0f ? Value / maxValue : 0f;

        [SerializeField] private float maxValue = 100f;

        public void ResetToMax()
        {
            Value = maxValue;
        }
        
        public bool IsZero() => Value <= 0f;
        
        public void Decrement(float amount)
        {
            Value = Mathf.Max(Value - amount, 0f);
        }
        
        public void Increment(float amount)
        {
            Value = Mathf.Min(Value + amount, maxValue);
        }

        public override void HardReset()
        {
            ResetOnValueChanged();
            value = maxValue;
        }
    }
}
