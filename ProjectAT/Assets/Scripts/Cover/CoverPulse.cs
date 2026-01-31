using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CoverPulse : MonoBehaviour
{
    private static readonly int Emission = Shader.PropertyToID("_Emission");

    [SerializeField]
    private Color baseColor = Color.yellow;
    [SerializeField]
    private float minIntensity = 2f;
    [SerializeField]
    private float maxIntensity = 6f;
    [SerializeField]
    private float speed = 4f;

    private DecalProjector decalProjector;
    private Material instanceMaterial;

    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();

        instanceMaterial = new Material(decalProjector.material);
        decalProjector.material = instanceMaterial;
    }

    private void OnDisable()
    {
        instanceMaterial.SetFloat(Emission, 0);
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        instanceMaterial.SetFloat(Emission, currentIntensity);
    }
}
