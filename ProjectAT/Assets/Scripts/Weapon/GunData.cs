using UnityEngine;

[CreateAssetMenu(fileName = "Gun Data", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject
{
    [SerializeField]
    private AudioClip shotClip; // �߻� �Ҹ�
    [SerializeField]
    private AudioClip reloadClip; // ������ �Ҹ�

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private GameObject hitPrefab;

    [SerializeField]
    private int damage = 25; // ���ݷ�

    [SerializeField]
    private int startRemainAmmo = 100; // ó���� �־��� ��ü ź��
    [SerializeField]
    private int magCapacity = 25; // źâ �뷮

    [SerializeField]
    private float timeBetFire = 0.12f; // �Ѿ� �߻� ����
    [SerializeField]
    private float reloadTime = 1.8f; // ������ �ҿ� �ð�

    [SerializeField]
    private float maxDistance = 100f;
    [SerializeField]
    private Sprite gunIcon;

    public AudioClip ShotClip => shotClip;
    public AudioClip ReloadClip => reloadClip;

    public GameObject BulletPrefab => bulletPrefab;
    public GameObject HitPrefab => hitPrefab;

    public int Damage => damage;

    public int StartRemainAmmo => startRemainAmmo;
    public int MagCapacity => magCapacity;

    public float TimeBetFire => timeBetFire;
    public float ReloadTime => reloadTime;

    public float MaxDistance => maxDistance;
    public Sprite GunIcon => gunIcon;
}