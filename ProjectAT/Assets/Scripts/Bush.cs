using System.Collections;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

    [SerializeField, Range(0f, 1f)]
    private float currentAlpha;
    [SerializeField]
    private float targetAlpha = 0.5f;
    [SerializeField]
    private float fadeDuration = 0.3f;

    [SerializeField]
    private Renderer meshRenderer;

    private MaterialPropertyBlock propertyBlock;
    private Color initialColor;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        initialColor = meshRenderer.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartFade(targetAlpha);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartFade(1f);
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

        Color color = initialColor;
        color.a = currentAlpha;

        propertyBlock.SetColor(ColorProperty, color);
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
    }
}
