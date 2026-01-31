using Unity.Networking.Transport;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GroundSkillIndicator : IndicatorBase
{
    DecalProjector decalProjector;
    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
    }


    public override void Show(float size = 1f)
    {
        decalProjector.size = new Vector3(size, size, decalProjector.size.z);
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }

    public override void UpdateIndicator(Vector3 position, Vector3 velocity)
    {
        transform.position = position;
    }
}
