using UnityEngine;

public class MouseDitherable : MonoBehaviour
{
    private static readonly int MouseDitherObjectEnabled = Shader.PropertyToID("_MouseDitherObjectEnabled");

    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private bool collectChildrenOnReset = true;
    [SerializeField] private bool warnIfShaderDoesNotSupportDither = true;

    private MaterialPropertyBlock propertyBlock;
    private bool warned;

    private void Reset()
    {
        if (collectChildrenOnReset)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void OnEnable()
    {
        ApplyObjectEnabled(1f);
    }

    private void OnDisable()
    {
        ApplyObjectEnabled(0f);
    }

    private void ApplyObjectEnabled(float value)
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        foreach (Renderer targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            WarnIfUnsupported(targetRenderer);

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(MouseDitherObjectEnabled, value);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void WarnIfUnsupported(Renderer targetRenderer)
    {
        if (!warnIfShaderDoesNotSupportDither || warned)
        {
            return;
        }

        foreach (Material material in targetRenderer.sharedMaterials)
        {
            if (material != null && material.HasProperty(MouseDitherObjectEnabled))
            {
                return;
            }
        }

        warned = true;
        Debug.LogWarning(
            $"{name} has MouseDitherable, but its materials do not expose _MouseDitherObjectEnabled.",
            this
        );
    }
}
