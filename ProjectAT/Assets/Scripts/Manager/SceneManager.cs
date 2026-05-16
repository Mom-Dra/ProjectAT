using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Lumin;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Start,
    Stage,
    End
}

public class SceneManager
{
    private CanvasGroup fadeCanvasGroup;
    private float fadeDuration = 3f;

    public void Initialize()
    {
        Addressables.InstantiateAsync("Assets/Prefabs/UI/GlobalFadeCanvas.prefab").Completed += FadeUILoaded;
    }

    private void FadeUILoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject fadeGo = handle.Result;
            fadeCanvasGroup = fadeGo.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup is null)
                Debug.Log("hahahahahahhahaha");

            Object.DontDestroyOnLoad(fadeGo);

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(SceneType sceneType)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)sceneType);
    }

    public void LoadSceneAsync(SceneType sceneType)
    {
        Managers.Instance.StartCoroutine(LoadSceneRoutine(sceneType));
    }

    private IEnumerator LoadSceneRoutine(SceneType sceneType)
    {
        // 1. Fade Out
        fadeCanvasGroup.blocksRaycasts = true; // 클릭 방지
        yield return fadeCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();

        // 2. Async Scene Loading (비동기 로딩 시작)
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)sceneType);
        op.allowSceneActivation = false; // 로딩이 다 되어도 바로 장면을 넘기지 않음

        // 로딩 진행률이 90%가 될 때까지 대기 (유니티에서 90%는 로딩 완료를 의미)
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // 잠시 대기 후 실제 씬 활성화
        yield return new WaitForSecondsRealtime(0.5f);
        op.allowSceneActivation = true;

        // 씬이 완전히 바뀔 때까지 한 프레임 대기
        yield return new WaitUntil(() => op.isDone);

        // 3. Fade In
        yield return fadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();
        fadeCanvasGroup.blocksRaycasts = false; // 클릭 해제
    }
}
