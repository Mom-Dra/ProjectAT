using UnityEngine;

[CreateAssetMenu(fileName = "EnemyIdentity", menuName = "Scriptable Objects/EnemyIdentity")]
public class EnemyIdentity : ScriptableObject
{
    [SerializeField]
    private string enemyName;

    public string EnemyName => enemyName;
}
