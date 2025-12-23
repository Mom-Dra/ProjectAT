using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DesignatedFire : Skill
{
    public DesignatedFire(PlayerSkillModule context, SkillData data) : base(context, data){}

    public override void OnChasing(Enemy target)
    {
        context.MyMovementModule.PlayerWalk(target.transform.position);
    }

    public override bool CanExecute(Enemy target)
    {
        return context.MyCombatModule.IsEnemyInWeaponSight(target);
    }

    public override void Execute(Enemy target)
    {
        //Snping
        Debug.Log("Designated Fire Executed!");

        if (target.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(500);

        CurrSkillTime = Time.time;
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
}
