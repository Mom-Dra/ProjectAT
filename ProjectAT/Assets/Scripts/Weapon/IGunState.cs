using System.Collections;
using UnityEngine;

// public interface IGunState
// {
//     static readonly IGunState ReadyState = new ReadyState();
//     static readonly IGunState WaitState = new WaitState();
//     static readonly IGunState EmptyState = new EmptyState();
//     static readonly IGunState ReloadState = new ReloadState();

//     void Enter(Gun gun);
//     void Fire(Gun gun);
// }

// public class ReadyState : IGunState
// {
//     public void Enter(Gun gun)
//     {

//     }

//     public void Fire(Gun gun)
//     {
//         if (gun.MagAmmo > 0) //Gun에서 자신의 총알을 체크하고 나서 PerformFire를 하는게 맞지 않나싶음. 안그러면 오토 리로딩을 구현하면 이런 중복 코드가 발생함.
//         {
//             gun.PerformFire();
//             gun.PlayFireRpc();

//             if(gun.MagAmmo > 0)
//             {
//                 gun.ChangeState(IGunState.WaitState);
//             }
//             else
//             {
//                 gun.ChangeState(IGunState.ReloadState);
//             }
//         }
//     }
// }

// public class WaitState : IGunState
// {
//     public void Enter(Gun gun)
//     {
//         Debug.Log($"WaitState : Entered. in {gun.GunData.TimeBetFire} seconds");
//         gun.StartCoroutine(WaitAndChangeState(gun));
//     }

//     public void Fire(Gun gun)
//     {

//     }

//     public void OnUpdate(Gun gun)
//     {
        
//     }

//     public void Reload(Gun gun){ }

//     private IEnumerator WaitAndChangeState(Gun gun)
//     {
//         yield return new WaitForSeconds(gun.GunData.TimeBetFire);

//         if (gun.MagAmmo > 0) gun.ChangeState(IGunState.ReadyState);
//         else gun.ChangeState(IGunState.EmptyState);
//     }
// }

// public class ReloadState : IGunState
// {
//     public void Enter(Gun gun)
//     {
//         gun.StartReloading();
//     }

//     public void Fire(Gun gun)
//     {
        
//     }
// }


// public class EmptyState : IGunState
// {
//     public void Enter(Gun gun)
//     {
//         Debug.Log("EmptyState : Entered.");
//     }

//     public void Fire(Gun gun)
//     {
//         Debug.Log("EmptyState : Can't fire. No ammo.");
//     }

//     public void Reload(Gun gun)
//     {
//         gun.StartReloading();
//     }
// }