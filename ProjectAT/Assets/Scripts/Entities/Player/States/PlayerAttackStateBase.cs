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
            case PlayerInputType.DesignatedFireKey:
                context.ChangeState(PlayerStateMachine.StateId.SkillTargeting);
                break;
        }
    }

    public override void OnUpdate()
    {
        var enemy = context.PlayerController.SelectedEnemy;
        var controller = context.PlayerController;

        if (enemy == null || controller.IsInAttackRange(enemy))  //적이 죽으면 enemy.IsAlive 도 있어야할듯.
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
            return;
        }
        else if (!controller.IsInAttackRange(enemy)) //적이 사정거리 밖으로 나가면
        {
            context.ChaseState.SetChaseState(controller.MyWeapon.Radius, PlayerStateMachine.StateId.Attack);
            context.ChangeState(PlayerStateMachine.StateId.Chase);
        }

        
        if (controller.SmoothRotateToTarget(enemy.transform.position)) //적이 정면에 없으면
        {
            controller.NormalAttackEnemy(enemy);
        }
    }
}
