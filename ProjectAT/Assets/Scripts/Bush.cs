using System.Collections;
using System.Collections.Generic;
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

    private readonly Dictionary<Transform, int> containedTargets = new Dictionary<Transform, int>();

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnTriggerEnter(Collider other)
    {
        IBushHideable bushHideable = other.GetComponentInParent<IBushHideable>();
        Transform target = GetContainedTarget(other, bushHideable);
        bool isFirstColliderInBush = AddContainedTarget(target);

        if (isFirstColliderInBush)
        {
            bushHideable?.EnterBush(this);
        }

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
        IBushHideable bushHideable = other.GetComponentInParent<IBushHideable>();
        Transform target = GetContainedTarget(other, bushHideable);
        bool isLastColliderOutOfBush = RemoveContainedTarget(target);

        if (isLastColliderOutOfBush)
        {
            bushHideable?.ExitBush(this);
        }

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

    public bool Contains(Transform target)
    {
        if (target is null) return false;

        return containedTargets.ContainsKey(target) || containedTargets.ContainsKey(target.root);
    }

    private static Transform GetContainedTarget(Collider other, IBushHideable bushHideable)
    {
        if (bushHideable is Component component) return component.transform;

        return other.transform.root;
    }

    private bool AddContainedTarget(Transform target)
    {
        if (target is null) return false;

        if (containedTargets.TryGetValue(target, out int count))
        {
            containedTargets[target] = count + 1;
            return false;
        }

        containedTargets.Add(target, 1);
        return true;
    }

    private bool RemoveContainedTarget(Transform target)
    {
        if (target is null) return false;
        if (!containedTargets.TryGetValue(target, out int count)) return false;

        count--;

        if (count > 0)
        {
            containedTargets[target] = count;
            return false;
        }

        containedTargets.Remove(target);
        return true;
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
