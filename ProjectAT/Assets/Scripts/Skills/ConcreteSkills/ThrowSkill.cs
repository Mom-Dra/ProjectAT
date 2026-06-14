using UnityEngine;

namespace EntitySkills{ 
    public class ThrowSkill : Skill
    {
        protected PlayerCombatModule combatModule;
        protected PlayerAnimator animator;
        protected ProjectileSkillData projectileData => skillData as ProjectileSkillData;

        public ThrowSkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
        {
            combatModule = context.MyCombatModule;
            animator = context.MyAnimModule;
        }

        public override float CalCulateFinalDamage()
        {
            return skillData.BaseDamage; //combatModule.CalculateDamage(skillData.BaseDamage);로 바꿔야할 필요가 있음.
        }

        public override float CalculateFinalRange()
        {
            return combatModule.ThrowRange;
        }

        public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
        {
            if (((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0)
            {
                target = null;
                point = hit.point;
                return true;
            }

            target = null;
            point = Vector3.zero;
            return false;
        }

        public override bool CanExecute(SkillContext skillContext)
        {
            return ExtraCastingCondition(skillContext);
        }

        public override bool ExtraCastingCondition(SkillContext skillContext)
        {
            return projectileData != null &&
                combatModule.CanThrowSomethingToPosition(
                    projectileData.ThrowingObjectPrefab,
                    skillContext.CastedPosition
                );
        }
    
        public override void Execute(SkillContext skillContext)
        {
            if (projectileData == null || projectileData.ThrowingObjectPrefab == null)
            {
                Debug.LogWarning($"{GetType().Name}: invalid ProjectileSkillData.");
                return;
            }

            InstantiateProjectile(skillContext, out ThrowProjectileBase projectile);

            if(projectile !=null)
            {
                SetupProjectile(projectile);
                combatModule.ThrowSomthingToTarget(projectile, skillContext.CastedPosition);
            }
        }

        protected void InstantiateProjectile(SkillContext skillContext, out ThrowProjectileBase projectile)
        {
            projectile = null;

            GameObject projectileObj = Object.Instantiate(
                projectileData.ThrowingObjectPrefab,
                skillContext.CastedPosition,
                Quaternion.identity
            );

            projectile = projectileObj.GetComponent<ThrowProjectileBase>();
        }

        protected virtual void SetupProjectile(ThrowProjectileBase projectile) //템플릿메서드 패턴을 이용해 필요한 경우 자식클래스에게 override로 위임.
        {
            if(projectile == null) return;
            projectile.Setup(projectileData.LandedNoiseRange, TargetLayer);
        }

        public override void OnCastingStart(SkillContext skillContext)
        {
            animator.WeaponMeshVisible(false);
            animator.PlayThrowAnimation();
        }

        public override void OnCastingEnd(SkillContext skillContext)
        {
            animator.WeaponMeshVisible(true);
        }
    }
}
