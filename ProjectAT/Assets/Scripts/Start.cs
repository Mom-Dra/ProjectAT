using UnityEngine;

public class Start : MonoBehaviour
{
    public void Foo()
    {
        Managers.Instance.SceneManager.LoadSceneAsync(SceneType.Stage);
    }
}
