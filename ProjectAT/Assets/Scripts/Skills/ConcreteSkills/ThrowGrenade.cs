using UnityEngine;
using SkillOptionInterfaces;

namespace EntitySkills
{
    public class ThrowGrenade : ThrowSkill, IInventoryCostSkill
    {
        private Inventory entityInventory;
        private GrenadeSkillData grenadeSkillData => skillData as GrenadeSkillData;

        public ThrowGrenade(PlayerSkillModule context, SkillData data) : base(context, data)
        {
            entityInventory = context.MyInventory;
        }

        public ItemData NeededItemData => grenadeSkillData?.NeededItemData;
        public int NeededItemAmount => grenadeSkillData?.NeededItemAmount ?? 0;

        public override bool CanActivate()
        {
            return base.CanActivate() && HasEnoughItem();
        }

        public override bool CanExecute(SkillContext skillContext)
        {
            return base.CanExecute(skillContext) && HasEnoughItem();
        }

        public bool HasEnoughItem()
        {
            return NeededItemData != null &&
                   entityInventory.GetItemCount(NeededItemData) >= NeededItemAmount;
        }

        public bool TryConsumeItem()
        {
            return NeededItemData != null &&
                   entityInventory.TryUseItem(NeededItemData, NeededItemAmount);
        }

        public override void Execute(SkillContext skillContext)
        {
            if (!HasEnoughItem())
                return;

            base.Execute(skillContext);
            TryConsumeItem();
        }

        protected override void SetupProjectile(ThrowProjectileBase projectile)
        {
            if (projectile == null)
            {
                Debug.LogWarning($"{GetType().Name}: projectile is null.");
                return;
            }
            
            base.SetupProjectile(projectile);
            if (projectile.TryGetComponent(out ProjectileGrenade grenade))
            {
                grenade.SetUp(
                    grenadeSkillData.BaseDamage,
                    grenadeSkillData.ExplosionRadius,
                    grenadeSkillData.FuseTime,
                    grenadeSkillData.ExplosionNoiseRadius
                );
            }
        }
    }
}