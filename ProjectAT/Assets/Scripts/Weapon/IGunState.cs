using System.Collections;
using UnityEngine;

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
            gun.PerformFire();
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