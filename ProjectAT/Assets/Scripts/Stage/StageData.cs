using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    public int stageNumber;
    public string stageName;
    public Sprite thumbnail;
    public string sceneName;

    public AudioClip bgm;
}
