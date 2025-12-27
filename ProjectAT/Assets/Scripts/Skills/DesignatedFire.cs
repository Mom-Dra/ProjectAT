using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DesignatedFire : Skill
{
    public DesignatedFire(PlayerSkillModule context, SkillData data) : base(context, data){}
    private Enemy targetEnemy;

    public override void OnChasing()
    {
        context.MyMovementModule.PlayerWalk(targetEnemy.transform.position);
    }

    public override bool CanSelectTarget(in RaycastHit hit)
    {
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0 
        && hit.collider.TryGetComponent<Enemy>(out targetEnemy))
        {
            return true;
        }

        return false;
    }

    public override bool CanExecute()
    {
        return context.MyCombatModule.IsEnemyInWeaponSight(targetEnemy)
        && context.MyMovementModule.PlayerRotateToward(targetEnemy.transform.position);
    }

    public override void Execute()
    {
        //Snping
        Debug.Log("Designated Fire Executed!");

        if (targetEnemy.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(500);

        //후처리
        CurrSkillTime = Time.time;
        targetEnemy = null;
    }


    public override void OnUiActivate()
    {
        Cursor.SetCursor(skillData.CursorSkin, new Vector2(skillData.CursorSkin.width * 0.5f, skillData.CursorSkin.height * 0.5f), CursorMode.Auto);
    }

    public override void OnUiDeactivate()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public override void OnUiUpdate()
    {
        //throw new System.NotImplementedException();
    }

    public override void CancelSkill()
    {
        targetEnemy = null;
    }
}
