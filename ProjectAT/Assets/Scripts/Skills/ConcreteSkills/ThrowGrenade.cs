
using UnityEngine;
using SkillOptionInterfaces;

namespace EntitySkills
{
    public class ThrowGrenade : ThrowSkill, IInventoryCostSkill
    {
        protected Inventory entityInventory = null;
        protected ItemData neededItemData = null;
        protected int neededItemAmount = 0;
        protected GrenadeSkillData grenadeSkillData => skillData as GrenadeSkillData;

        public ThrowGrenade(PlayerSkillModule context, SkillData data) : base(context, data)
        {
            entityInventory = context.MyInventory;
        }

        public ItemData NeededItemData => throw new System.NotImplementedException();
        public int NeededItemAmount => throw new System.NotImplementedException();

        public bool HasEnoughItem()
        {
            return true;
        }

        public bool TryConsumeItem()
        {
            return true;
        }

        protected override void SetupProjectile(ThrowProjectileBase projectile) 
        {
            base.SetupProjectile(projectile);

            if(projectile.TryGetComponent(out ProjectileGrenade grenade))
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