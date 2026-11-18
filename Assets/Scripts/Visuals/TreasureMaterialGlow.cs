using UnityEngine;

public class TreasureMaterialGlow : MonoBehaviour
{
    [Header("Decorative treasure materials only")]
    [SerializeField] private Material[] materials;

    [Header("Glow")]
    [SerializeField, Min(0f)] private float minimumGlow = 0.05f;
    [SerializeField, Min(0f)] private float maximumGlow = 0.7f;
    [SerializeField, Min(0.1f)] private float pulseDuration = 3f;

    private Color[] originalEmission;
    private Color[] glowColours;
    private bool[] emissionWasEnabled;

    private static readonly int BaseColor =
        Shader.PropertyToID("_BaseColor");

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");

    private void OnEnable()
    {
        if (materials == null)
            return;

        originalEmission = new Color[materials.Length];
        glowColours = new Color[materials.Length];
        emissionWasEnabled = new bool[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];

            if (material == null || !material.HasProperty(EmissionColor))
                continue;

            originalEmission[i] = material.GetColor(EmissionColor);
            emissionWasEnabled[i] =
                material.IsKeywordEnabled("_EMISSION");

            glowColours[i] = material.HasProperty(BaseColor)
                ? material.GetColor(BaseColor)
                : Color.white;

            material.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (materials == null || glowColours == null)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];

            if (material == null || !material.HasProperty(EmissionColor))
                continue;

            float phase = i * 1.7f;
            float wave = Mathf.Sin(
                Time.time * Mathf.PI * 2f / pulseDuration + phase
            );

            float pulse = Mathf.Pow(wave * 0.5f + 0.5f, 3f);
            float strength = Mathf.Lerp(
                minimumGlow, maximumGlow, pulse
            );

            material.SetColor(
                EmissionColor,
                glowColours[i] * strength
            );
        }
    }

    private void OnDisable()
    {
        if (materials == null || originalEmission == null)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];

            if (material == null || !material.HasProperty(EmissionColor))
                continue;

            material.SetColor(EmissionColor, originalEmission[i]);

            if (!emissionWasEnabled[i])
                material.DisableKeyword("_EMISSION");
        }
    }
}