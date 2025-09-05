using System.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    public abstract void AttackRpc();
}

public interface IGunState
{
    static readonly IGunState ReadyState = new ReadyState();
    static readonly IGunState WaitState = new WaitState();
    static readonly IGunState ReloadState = new ReloadState();
    static readonly IGunState EmptyState = new EmptyState();

    void Enter(Gun gun);
    void Fire(Gun gun);
    void Reload(Gun gun);
}

// 잠깐 대기는 서버에서만
// 장전은 클라에서도!

public class ReadyState : IGunState
{
    public void Enter(Gun gun)
    {
        
    }

    public void Fire(Gun gun)
    {
        // 현재 탄창이 0이면 가방 탄창 0 -> EmptyState
        // 현재 탄창이 0 -> 가방 탄창 0 이상 -> Reload
        if (gun.MagAmmo > 0)
        {
            gun.PlayFireRpc(); // 실제 발사 로직 (총알 감소, 이펙트 등)

            // 마지막 총알을 쐈고, 남은 총알이 있다면 자동 재장전
            if (gun.MagAmmo == 0 && gun.RemainAmmo > 0)
                gun.ChangeState(IGunState.ReloadState);
            // 아니라면 발사 후 대기 상태로
            else gun.ChangeState(IGunState.WaitState);
        }
    }

    public void Reload(Gun gun)
    {
        if (gun.CanReload())
            gun.ChangeState(IGunState.ReloadState);
    }
}

public class WaitState : IGunState
{
    public void Enter(Gun gun)
    {
        gun.StartCoroutine(WaitAndChangeState(gun));
    }

    public void Fire(Gun gun)
    {

    }

    public void Reload(Gun gun)
    {
        
    }

    private IEnumerator WaitAndChangeState(Gun gun)
    {
        yield return new WaitForSeconds(gun.GunData.TimeBetFire);

        if (gun.MagAmmo > 0) gun.ChangeState(IGunState.ReadyState);
        else gun.ChangeState(IGunState.EmptyState);
    }
}

public class ReloadState : IGunState
{
    public void Enter(Gun gun)
    {
        gun.PlayReloadRpc();
        gun.StartCoroutine(ReloadAndChangeState(gun));
    }

    public void Fire(Gun gun)
    {

    }

    public void Reload(Gun gun)
    {

    }

    private IEnumerator ReloadAndChangeState(Gun gun)
    {
        yield return new WaitForSeconds(gun.GunData.ReloadTime);

        gun.PerformReload();
        gun.ChangeState(IGunState.ReadyState);
    }
}

public class EmptyState : IGunState
{
    public void Enter(Gun gun)
    {

    }

    public void Fire(Gun gun)
    {

    }

    public void Reload(Gun gun)
    {

    }
}

public class Gun : Weapon
{
    [SerializeField]
    private GunData gunData;
    public GunData GunData => gunData;

    private IGunState gunState;

    private LineRenderer lineRenderer;
    private AudioSource audioSource;

    private NetworkVariable<int> remainAmmo; // 남은 전체 탄약
    private NetworkVariable<int> magAmmo; // 탄창에 남은 탄약

    internal int RemainAmmo { get => remainAmmo.Value; set => remainAmmo.Value = value; }
    internal int MagAmmo { get => magAmmo.Value; set => magAmmo.Value = value; }

    // Animator 쪽 실제 에셋 붙여보고 생각해보자

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            remainAmmo.Value = gunData.StartRemainAmmo;
            magAmmo.Value = gunData.MagCapacity;
        }

        remainAmmo.OnValueChanged += RemainAmmoValueChanged;
        magAmmo.OnValueChanged += MagAmmoValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        remainAmmo.OnValueChanged -= RemainAmmoValueChanged;
        magAmmo.OnValueChanged -= MagAmmoValueChanged;
    }

    private void RemainAmmoValueChanged(int previousRemainAmmo, int currentRemainAmmo)
    {
        // UI Update...
    }

    private void MagAmmoValueChanged(int previousMagAmmo, int currentMagAmmo)
    {
        // UI Update...
    }

    [Rpc(SendTo.Server)]
    public override void AttackRpc()
    {
        gunState.Fire(this);
    }

    public void Reload()
    {
        gunState.Reload(this);
    }

    internal void ChangeState(IGunState gunState)
    {
        this.gunState = gunState;
        gunState.Enter(this);
    }

    internal bool CanReload()
    {
        return RemainAmmo > 0 && MagAmmo < gunData.MagCapacity;
    }

    internal void PerformReload()
    {
        int neededAmmo = gunData.MagCapacity - MagAmmo;
        int ammoToMove = Mathf.Min(neededAmmo, RemainAmmo);

        MagAmmo += ammoToMove;
        RemainAmmo -= ammoToMove;
    }

    // Client 쪽에서 Effect만 재생할 거임
    [Rpc(SendTo.ClientsAndHost)]
    internal void PlayFireRpc()
    {
        Debug.Log("PlayFireRpc");
        audioSource.PlayOneShot(gunData.ShotClip);
    }

    [Rpc(SendTo.ClientsAndHost)]
    internal void PlayReloadRpc()
    {
        Debug.Log("PlayReloadRpc");
        audioSource.PlayOneShot(gunData.ReloadClip);

        // Reload Animation
    }
    // Ready -> 
}

public class Grande : Weapon
{
    public override void AttackRpc()
    {
        throw new System.NotImplementedException();
    }
}