using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAT.FieldUI
{
    public class RadialFillImageView : FieldUI
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text fallbackLabel;
        [SerializeField] private string fallbackText = "!";

        private BuffInstance boundBuff;
        private Sprite boundFallbackIcon;

        public bool IsBound => boundBuff != null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ConfigureIconImage();
        }
#endif

        public void Bind(BuffInstance buff, Sprite fallbackIcon)
        {
            if (buff == null)
            {
                Unbind();
                return;
            }

            boundBuff = buff;
            boundFallbackIcon = fallbackIcon;

            RefreshContent();
            SetVisible(true);
        }

        public void Unbind()
        {
            boundBuff = null;
            boundFallbackIcon = null;

            ClearVisual();
            SetVisible(false);
        }

        protected override bool ShouldShow()
        {
            return boundBuff != null;
        }

        protected override void BindView(RectTransform view)
        {
            ConfigureIconImage();
            ClearVisual();
        }

        protected override void RefreshContent()
        {
            if (boundBuff == null)
            {
                return;
            }

            SetIcon(boundBuff.Icon, boundFallbackIcon);
            SetFillAmount(boundBuff.RemainingRatio);
        }

        private void SetIcon(Sprite icon, Sprite fallbackIcon)
        {
            Sprite visibleIcon = icon != null ? icon : fallbackIcon;

            if (iconImage != null)
            {
                iconImage.sprite = visibleIcon;
                iconImage.enabled = visibleIcon != null;
            }

            if (fallbackLabel != null)
            {
                fallbackLabel.text = fallbackText;
                fallbackLabel.enabled = visibleIcon == null;
            }
        }

        private void SetFillAmount(float normalizedAmount)
        {
            if (iconImage != null)
            {
                iconImage.fillAmount = Mathf.Clamp01(normalizedAmount);
            }
        }

        private void ClearVisual()
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.fillAmount = 0f;
                iconImage.enabled = false;
            }

            if (fallbackLabel != null)
            {
                fallbackLabel.text = fallbackText;
                fallbackLabel.enabled = false;
            }
        }

        private void ConfigureIconImage()
        {
            if (iconImage == null)
            {
                return;
            }

            iconImage.type = Image.Type.Filled;
            iconImage.fillMethod = Image.FillMethod.Radial360;
            iconImage.fillOrigin = (int)Image.Origin360.Top;
            iconImage.fillClockwise = false;
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
        }
    }
}
