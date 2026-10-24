using CCLBStudio.ScriptableValue;
using UnityEngine;

namespace Game.Objective
{

public class RocketMaterialModifier : MonoBehaviour
{
    [SerializeField] private FloatValue rocketHealth;

    [Header("Paramètres d'effet maximal (Santé = 0)")]
    [ColorUsage(false, true)] 
    [SerializeField] private Color maxEmissionColor = Color.red;
    [SerializeField] private Vector3 maxVertexAnimationFrequency = new Vector3(2f, 2f, 2f);

    private Material _material;
    private float _initialHealth;

    // Identifiants internes des propriétés du shader MK Toon
    private static readonly int EmissionColorProp = Shader.PropertyToID("_EmissionColor");
    private static readonly int VertexAnimIntensityProp = Shader.PropertyToID("_VertexAnimationIntensity");
    private static readonly int VertexAnimFrequencyProp = Shader.PropertyToID("_VertexAnimationFrequency");

    private void Awake()
    {
        if (rocketHealth == null)
        {
            Debug.LogError("FloatValue rocketHealth n'est pas assigné.", this);
            return;
        }

        _initialHealth = rocketHealth.Value;
        rocketHealth.OnValueChanged += OnRocketHealthChanged;

        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("RocketMaterialModifier requires a Renderer component.", this);
            return;
        }

        var material = renderer.material;
        if (!material)
        {
            Debug.LogError("Renderer does not have a material.", this);
            return;
        }

        _material = material;

        // Initialisation de l'affichage avec la santé de départ
        OnRocketHealthChanged(_initialHealth);
    }

    private void OnDestroy()
    {
        if (rocketHealth != null)
        {
            rocketHealth.OnValueChanged -= OnRocketHealthChanged;
        }
    }

    private void OnRocketHealthChanged(float newValue)
    {
        if (!_material) return;

        // Valeur normalisée entre 0 et 1
        float normalizedHealth = _initialHealth > 0f ? Mathf.Clamp01(newValue / _initialHealth) : 0f;

        // Facteur inversé : 0 quand la santé est pleine (1), 1 quand la santé est vide (0)
        float effectFactor = 1f - normalizedHealth;

        // 1. Ajustement de l'émission (HDR)
        Color targetEmission = maxEmissionColor * effectFactor;
        _material.SetColor(EmissionColorProp, targetEmission);

        // 2. Ajustement de l'intensité de Vertex Animation
        float targetIntensity = effectFactor;
        _material.SetFloat(VertexAnimIntensityProp, targetIntensity);

        // 3. Ajustement de la fréquence de Vertex Animation (X, Y, Z)
        Vector4 targetFrequency = (Vector4)(maxVertexAnimationFrequency * effectFactor);
        _material.SetVector(VertexAnimFrequencyProp, targetFrequency);
    }
}
}