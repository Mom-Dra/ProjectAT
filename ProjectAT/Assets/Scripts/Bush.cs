using System.Collections;
using UnityEngine;

public class Bush : MonoBehaviour, IFadeable
{
    private static readonly int DitherProperty = Shader.PropertyToID("_DitherStrength");

    private const float OPAQUE = 2;

    [SerializeField, Range(0f, 2f)]
    private float currentAlpha;
    [SerializeField]
    private float targetAlpha = 0.5f;
    [SerializeField]
    private float fadeDuration = 0.3f;

    [SerializeField]
    private Renderer meshRenderer;

    private MaterialPropertyBlock propertyBlock;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IStealthable stealthable))
        {
            stealthable.SetVisibility(true);
        }

        if (other.TryGetComponent(out IFadeable fadeable))
        {
            fadeable.FadeOut();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            FadeOut();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IStealthable stealthable))
        {
            stealthable.SetVisibility(false);
        }

        if (other.TryGetComponent(out IFadeable fadeable))
        {
            fadeable.FadeIn();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            FadeIn();
        }
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
        float startAlpha = currentAlpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / duration;

            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, ratio);

            ApplyAlpha(currentAlpha);

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
