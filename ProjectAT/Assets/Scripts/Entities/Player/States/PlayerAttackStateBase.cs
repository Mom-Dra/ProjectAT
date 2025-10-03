using UnityEditor;
using UnityEngine;

public class PlayerAttackStateBase : EntityState
{
    public PlayerAttackStateBase(PlayerStateMachine cxt) : base(cxt){ }

    public override void Enter()
    {
        context.PlayerController.StopMoving();
        context.PlayerController.MyAnim.SetBool("isFiring", true);
    }

    public override void Exit()
    {
        context.PlayerController.MyAnim.SetBool("isFiring", false);
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.RightClick:
                PlayerStateMachine.StateId nextState = context.PlayerController.CalCulateNextStateByMouseRaycast();
                if (nextState != PlayerStateMachine.StateId.Chase) //공격이 아니면
                {
                    context.PlayerController.SetTargetEnemy(null); //타겟 초기화
                    context.ChangeState(nextState);
                }
                break;
        }
    }

    public override void OnUpdate()
    {
        var enemy = context.PlayerController.SelectedEnemy;
        var controller = context.PlayerController;

        if (enemy == null)  //적이 죽으면 enemy.IsAlive 도 있어야할듯.
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
            return;
        }
        else if (!controller.IsInAttackRange(enemy)) //적이 사정거리 밖으로 나가면
        {
            context.ChaseState.SetChaseState(controller.MyWeapon.Radius, PlayerStateMachine.StateId.Attack);
            context.ChangeState(PlayerStateMachine.StateId.Chase);
        }

        Vector3 playerToEnemy = (enemy.transform.position - controller.transform.position);
        Vector3 playerForward = controller.transform.forward;
        playerToEnemy.y = playerForward.y = 0;

        if (Vector3.Angle(playerForward, playerToEnemy) >= 0.01f) //적이 정면에 없으면
        {
            Quaternion lookRotation = Quaternion.LookRotation(playerToEnemy);
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
        else
        {
            controller.NormalAttackEnemy(enemy);
        }
            return;
    }
}
