using UnityEngine;

[CreateAssetMenu(fileName = "Gun Data", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject
{
    [Header("Gun Prefabs")]
    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private GameObject hitPrefab;

    [Header("Gun Stats")]
    [SerializeField]
    private int damage = 25; // ���ݷ�

    [SerializeField]
    private int startRemainAmmo = 100; // ó���� �־��� ��ü ź��
    [SerializeField]
    private int magCapacity = 25; // źâ �뷮

    [SerializeField]
    private float timeBetFire = 0.12f; // �Ѿ� �߻� ����
    [SerializeField] 
    private float fullAutoFireRate = 0.1f;
    [SerializeField]
    private float reloadTime = 1.8f; // ������ �ҿ� �ð�

    [SerializeField]
    private float maxDistance = 100f;
    [Header("Visuals")]
    [SerializeField]
    private Sprite gunIcon;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip shotClip;
    [SerializeField]
    private AudioClip reloadStartClip;
    [SerializeField] 
    private AudioClip reloadEndClip;


    public AudioClip ShotClip => shotClip;
    public AudioClip ReloadStartClip => reloadStartClip;
    public AudioClip ReloadEndClip => reloadEndClip;

    public GameObject BulletPrefab => bulletPrefab;
    public GameObject HitPrefab => hitPrefab;

    public int Damage => damage;

    public int StartRemainAmmo => startRemainAmmo;
    public int MagCapacity => magCapacity;

    public float TimeBetFire => timeBetFire;
    public float FullAutoFireRate => fullAutoFireRate;
    public float ReloadTime => reloadTime;

    public float MaxDistance => maxDistance;
    public Sprite GunIcon => gunIcon;
}