using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using UnityEngine.Assertions;

public class Test2 : MonoBehaviour
{
    [SerializeField] private BuffData[] moveSpeedBuffs;

    private void Start()
    {
        ApplyMoveSppedBuff(gameObject);
    }

    private void ApplyMoveSppedBuff(GameObject target)
    {
        if (target.TryGetComponent(out BuffModule buffModule))
        {
            foreach (BuffData buffData in moveSpeedBuffs)
                buffModule.AddBuff(buffData);
        }
    }


    private CanvasGroup canvasGroup;
    private Sequence fadeSequence;

    private void Awake()
    {
        // canvasGroup = GetComponent<CanvasGroup>();

        // canvasGroup.alpha = 0f;
        // canvasGroup.blocksRaycasts = false;

        // Transform abc = null;

        // Assert.IsNotNull(abc);
    }

    private void OnEnable()
    {
        // Managers.Instance.InputManager.onMouseLeftClicked += Foo;
    }

    private void Foo()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End);
    }

    // public void PlayFadeInOut(float duration, Action onScreenCovered)
    // {
    //     fadeSequence?.Kill();
    //     fadeSequence = DOTween.Sequence();

    //     fadeSequence.SetUpdate(true);

    //     canvasGroup.blocksRaycasts = true;

    //     fadeSequence.Append(canvasGroup.DOFade(1f, duration)).SetEase(Ease.InOutQuad);


    // }

    // public void PlayFadeInOut(float duration, Action onScreenCovered)
    // {
    //     fadeSequence.?.Kill();

    //     fadeSequence = DOTween.Sequence();
    //     fadeSequence.SetUpdate(true);
    // }

    // public void PlayFadeInOut(float duration, Action onScreenCovered)
    // {
    //     fadeSequence?.Kill();
    //     fadeSequence = DOTween.Sequence();
    //     fadeSequence.SetUpdate(true);
    // }

    // public void PlayFadeInOut(float duration, Action onScreenCovered)
    // {
    //     fadeSequence?.Kill();

    //     fadeSequence = DOTween.Sequence();

    //     fadeSequence.SetUpdate(true);

    //     canvasGroup.blocksRaycasts = true;

    //     fadeSequence.Append(canvasGroup.DOFade(1f, duration)).SetEase(Ease.InOutQuad);

    //     fadeSequence.AppendCallback(() =>
    //     {
    //         onScreenCovered?.Invoke();
    //     });

    //     fadeSequence.Append(canvasGroup.DOFade(0f, duration)).SetEase(Ease.InOutQuad);

    //     fadeSequence.OnComplete(() =>
    //     {
    //         canvasGroup.blocksRaycasts = false;
    //         fadeSequence = null;
    //     });
    // }

    private void Oestroy()
    {
        fadeSequence?.Kill();
    }
}

