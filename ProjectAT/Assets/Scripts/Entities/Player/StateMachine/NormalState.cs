using UnityEngine;
using PlayerStateMachine;
using PlayerStatusCapabilities;
using System.Data;

public class NormalState : PlayerState, ILeftClickHandler, IRightClickHandler, ISkillInputHandler
{
    private PlayerCoverModule myCoverModule;
    private PlayerSkillModule mySkillModule;

    public NormalState(PlayerController playerController) : base(playerController)
    {
        myCoverModule = context.MyCoverModule;
        mySkillModule = context.MySkillModule;
    }

    public override void OnEnter()
    {
        context.CancelEnemySelect();
    }
    
    public override void OnUpdate()
    {
        if (context.SelectedEnemy != null)
        {
            context.EnemyAttackingSequence();
        }
    }

    public override void OnExit()
    {
        
    }

    public void OnLeftClick(RaycastHit castedObject)
    {
        if (mySkillModule.IsTargetting && mySkillModule.CanSelectTarget(castedObject, out GameObject target, out Vector3 point))
        {
            mySkillModule.ActivateSelectedSkill();
            mySkillModule.SetUpSkillContext(target, point);
            context.ChangeState(PlayerStateType.SkillChase);
        }
    }

    public void OnRightClick(RaycastHit castedObject)
    {        
        if(mySkillModule.IsTargetting) // 스킬 UI 중 우클릭 시 UI 해제. 만약 이 로직이 모든 State들의 RightClick에서 공통적으로 일어나면 아예 PlayerController에서 처리하기.
        {
            mySkillModule.CancelTargettingMode();
            return;
        }

        switch (castedObject.collider.gameObject.layer)
        {
            case 6: //Ground Layer
                context.PlayerMove(castedObject.point, false);
                break;
            case 7: //Enemy Layer
                context.SetTargetEnemy(castedObject.collider.GetComponent<Enemy>());
                break;
            case 10: //Indicator Layer
                context.PlayerMove(castedObject.point, true);
                break;
            case 11: // CoverPoint Layer
                if (castedObject.transform.TryGetComponent(out CoverPoint coverPoint))
                    myCoverModule.StartMoveToCover(coverPoint);
                break;
            case 13: // Interactable Layer
                context.MyInteractionModule.HandleRightClick();
                break;
            default:
                break;
        }
    }

    public void OnSkillInput(SkillNumber skillNumber)
    {
        if(!mySkillModule.IsTargetting)
        {
            context.MySkillModule.ActivateTargettingMode(skillNumber);
        }
        else
        {
            mySkillModule.CancelTargettingMode();
        }
    }
}
