using System.Collections;
using UnityEngine;

public class EndScene : MonoBehaviour
{
    private readonly static WaitForSeconds wait = new WaitForSeconds(3f);

    private void Start()
    {
        StartCoroutine(WaitCoroutine());
    }

    private IEnumerator WaitCoroutine()
    {
        yield return wait;

        GoStartScene();
    }

    private void GoStartScene()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.Start);
    }
}
