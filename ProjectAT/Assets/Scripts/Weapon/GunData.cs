using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/GunData", fileName = "Gun Data")]
public class GunData : ScriptableObject
{
    [SerializeField]
    private AudioClip shotClip; // 발사 소리
    [SerializeField]
    private AudioClip reloadClip; // 재장전 소리

    [SerializeField]
    private float damage = 25; // 공격력

    [SerializeField]
    private int startRemainAmmo = 100; // 처음에 주어질 전체 탄약
    [SerializeField]
    private int magCapacity = 25; // 탄창 용량

    [SerializeField]
    private float timeBetFire = 0.12f; // 총알 발사 간격
    [SerializeField]
    private float reloadTime = 1.8f; // 재장전 소요 시간

    public AudioClip ShotClip => shotClip;
    public AudioClip ReloadClip => reloadClip;

    public float Damage => damage;

    public int StartRemainAmmo => startRemainAmmo;
    public int MagCapacity => magCapacity;

    public float TimeBetFire => timeBetFire;
    public float ReloadTime => reloadTime;
}