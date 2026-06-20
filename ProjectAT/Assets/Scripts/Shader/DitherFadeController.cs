using System.Collections;
using UnityEngine;

public class DitherFadeController : MonoBehaviour
{
    private static readonly int DitherProperty = Shader.PropertyToID("_DitherStrength");

    [SerializeField, Range(0f, 1f)] private float visibleStrength = 1f;
    [SerializeField, Range(0f, 1f)] private float hiddenStrength = 0.5f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.3f;
    [SerializeField] private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine fadeCoroutine;
    private float currentStrength;
    private bool isHidden;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        currentStrength = visibleStrength;

        renderers = GetComponentsInChildren<Renderer>();

        Apply(currentStrength);
    }

    public void SetHidden(bool isHidden)
    {
        if (this.isHidden == isHidden) return;

        this.isHidden = isHidden;

        StartFade(isHidden ? hiddenStrength : visibleStrength);
    }

    private void StartFade(float target)
    {
        if (fadeCoroutine is not null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(target));
    }

    private IEnumerator FadeCoroutine(float target)
    {
        float start = currentStrength;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float ratio = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            currentStrength = Mathf.Lerp(start, target, ratio);

            Apply(currentStrength);

            yield return null;
        }

        currentStrength = target;
        Apply(currentStrength);
        fadeCoroutine = null;
    }

    private void Apply(float strength)
    {
        if (renderers is null) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer is null) continue;

            Debug.Log($"SetFloat: {strength}");

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DitherProperty, strength);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
