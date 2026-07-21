using UnityEngine;

public class StartScene : MonoBehaviour
{
    [SerializeField] private AudioClip mainBgm;

    public void Start()
    {
        // Managers.Instance.SceneManager.LoadSceneAsync(SceneType.Stage);
        Managers.Instance.SoundManager.PlayBgm(mainBgm, 0.1f);
    }
}
