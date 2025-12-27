using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CoverPulse : MonoBehaviour
{
    private DecalProjector decalProjector;
    [SerializeField]
    private Color baseColor = Color.yellow;
    [SerializeField]
    private float minIntensity = 2f;
    [SerializeField]
    private float maxIntensity = 6f;
    [SerializeField]
    private float speed = 4f;

    private Material instanceMaterial;
    private Color initialColor;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();

        initialColor = decalProjector.material.GetColor(EmissionColor);

        instanceMaterial = new Material(decalProjector.material);
        decalProjector.material = instanceMaterial;
    }

    private void OnDisable()
    {
        instanceMaterial.SetColor(EmissionColor, initialColor);
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        Color finalColor = baseColor * Mathf.Pow(2, currentIntensity);
        instanceMaterial.SetColor(EmissionColor, finalColor);
    }
}
