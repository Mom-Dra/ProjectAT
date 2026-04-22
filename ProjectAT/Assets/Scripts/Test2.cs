using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class Test2 : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Sequence fadeSequence;

    private void Awake()
    {
        // canvasGroup = GetComponent<CanvasGroup>();

        // canvasGroup.alpha = 0f;
        // canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        // Managers.Instance.InputManager.onMouseLeftClicked += Foo;
    }

    private void Foo()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.End);
    }

    private void Start()
    {
        // DOTween.Init();
        // transform.DOMove(new Vector3(5f, 5f, 0f), 2f);

        // transform.DOScale(new Vector3(2f, 2f, 2f), 1f).SetEase(Ease.InBounce);


        // Managers.Instance.EventManager.Subscribe<int>(EventType.Last, Foo);


        // PlayFadeInOut(5f, () => Debug.Log("KKKKKKey!"));


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

