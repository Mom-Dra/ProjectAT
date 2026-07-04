namespace PlayerStateMachine
{
    public class StunnedState : PlayerState
    {
        public StunnedState(PlayerController playerController) : base(playerController)
        {
        }

        public override void OnEnter()
        {
            context.MyMovementModule.PlayerMoveStop();
            context.AimingEnemy(false);
            context.MyCombatModule.RequestCancelReload();
            context.MyAnimModule.SetCrouch(false);
            context.MyAnimModule.WeaponMeshVisible(true);
            context.MyAnimModule.CancelAnimation();
        }

        public override void OnUpdate()
        {
            context.MyMovementModule.PlayerMoveStop();
        }

        public override void OnExit()
        {
        }
    }
}
