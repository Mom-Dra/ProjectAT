using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.Rendering;

public interface IStealthable
{
    bool IsHidden { get; }
    void SetVisibility(bool hidden);
}

public class StealthModule : MonoBehaviour, IStealthable, IFadeable
{
    private static readonly int DitherProperty = Shader.PropertyToID("_DitherStrength");
    private const int OPAQUE = 1;

    [SerializeField, Range(0f, 1f)]
    private float currAlpha;

    [SerializeField, Range(0f, 1f)]
    private float targetAlpha;

    [SerializeField, Range(0f, 1f)]
    private float fadeDuration;

    [SerializeField]
    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private bool isHidden;

    private Coroutine fadeCoroutine;

    public bool IsHidden => isHidden;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void SetVisibility(bool isHidden)
    {
        this.isHidden = isHidden;
    }

    private void StartFade(float target)
    {
        if (fadeCoroutine is not null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(target, fadeDuration));
    }

    private void ApplyAlpha(float alpha)
    {
        meshRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(DitherProperty, alpha);

        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration)
    {
        float elapsed = 0f;
        float startAlpha = currAlpha;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / duration;

            currAlpha = Mathf.Lerp(startAlpha, targetAlpha, ratio);

            ApplyAlpha(currAlpha);

            yield return null;
        }

        ApplyAlpha(targetAlpha);
    }

    public void FadeOut()
    {
        StartFade(targetAlpha);
    }

    public void FadeIn()
    {
        StartFade(OPAQUE);
    }
}
