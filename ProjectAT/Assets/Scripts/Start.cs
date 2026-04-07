using UnityEngine;

public class Start : MonoBehaviour
{
    public void Foo()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneType.Stage);
    }
}
