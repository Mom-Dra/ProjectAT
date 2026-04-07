using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Start,
    Stage,
    End
}

public class SceneManager
{
    public void LoadScene(SceneType sceneType)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)sceneType);
    }
}
