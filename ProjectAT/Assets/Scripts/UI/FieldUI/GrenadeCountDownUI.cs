using UnityEngine;

namespace ProjectAT.FieldUI
{
    public class GrenadeCountdownUI : FieldUI
    {
        [Header("Grenade Countdown")]
        [SerializeField] private ProjectileGrenade grenade;
        private CountdownView countdownView;

        protected override void ResolveReferences()
        {
            base.ResolveReferences();

            if (grenade == null)
            {
                grenade = GetComponent<ProjectileGrenade>();
            }
        }

        protected override void BindView(RectTransform view)
        {
            countdownView = view.GetComponent<CountdownView>();

            if (countdownView == null)
            {
                Debug.LogError($"[{nameof(GrenadeCountdownUI)}] " + "View 프리팹에 CountdownView 없습니다.", this);
            }
        }
        protected override bool ShouldShow()
        {
            return grenade != null 
                && countdownView != null 
                && grenade.IsFuseRunning;
        }

        protected override void RefreshContent()
        {
            countdownView.SetRemainingSeconds(grenade.RemainingFuseTime);
        }
    }
}