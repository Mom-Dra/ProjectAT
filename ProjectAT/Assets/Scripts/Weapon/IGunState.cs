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

// ��� ���� ����������
// ������ Ŭ�󿡼���!
public class ReadyState : IGunState
{
    public void Enter(Gun gun)
    {

    }

    public void Fire(Gun gun)
    {
        // ���� źâ�� 0�̸� ���� źâ 0 -> EmptyState
        // ���� źâ�� 0 -> ���� źâ 0 �̻� -> Reload
        if (gun.MagAmmo > 0)
        {
            gun.PerformFire();
            gun.PlayFireRpc(); // ���� �߻� ���� (�Ѿ� ����, ����Ʈ ��)

            // ������ �Ѿ��� ����, ���� �Ѿ��� �ִٸ� �ڵ� ������
            if (gun.MagAmmo == 0 && gun.RemainAmmo > 0)
                gun.ChangeState(IGunState.ReloadState);
            // �ƴ϶�� �߻� �� ��� ���·�
            else {
            gun.ChangeState(IGunState.WaitState);
            }
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
        Debug.Log($"WaitState : Entered. in {gun.GunData.TimeBetFire} seconds");
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
        Debug.Log("EmptyState : Entered.");
    }

    public void Fire(Gun gun)
    {

    }

    public void Reload(Gun gun)
    {

    }
}