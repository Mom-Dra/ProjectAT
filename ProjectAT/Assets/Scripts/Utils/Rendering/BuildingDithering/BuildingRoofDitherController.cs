using System.Collections;
using UnityEngine;

namespace Rendering.BuildingDithering
{
    public class BuildingRoofDitherController : MonoBehaviour
    {
        private static readonly int RoofDitherAlpha = Shader.PropertyToID("_RoofDitherAlpha");
        private static readonly int MouseDitherObjectEnabled = Shader.PropertyToID("_MouseDitherObjectEnabled");

        [Header("Roof Dithering Settings")]
        [SerializeField] private Renderer[] roofRenderers;
        [SerializeField, Range(0f, 1f)] private float targetAlpha = 0.1f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        [Header("Building Trigger Settings")]
        [SerializeField] private BuildingInteriorTrigger interiorTriggers;
        [SerializeField] private LayerMask playerLayer;

        private MaterialPropertyBlock propertyBlock;
        private Coroutine fadeCoroutine;
        private bool isMouseOverRoof;
        private int playerInsideCount;
        private float currentAlpha;

        internal bool ShouldDither => isMouseOverRoof || playerInsideCount > 0;

        public BuildingInteriorTrigger InteriorTriggers => interiorTriggers;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            ApplyAlpha(1f);
            interiorTriggers.ResetParameters(this, playerLayer);
        }

        private void OnEnable()
        {
            if (interiorTriggers != null)
            {
                interiorTriggers.OnPlayerEnter += AddPlayerInside;
                interiorTriggers.OnPlayerExit += RemovePlayerInside;
            }
        }

        private void OnDisable()
        {
            if(interiorTriggers != null)
            {
                interiorTriggers.OnPlayerEnter -= AddPlayerInside;
                interiorTriggers.OnPlayerExit -= RemovePlayerInside;
            }
        }

        public void SetMouseOverRoof(bool value)
        {
            if(isMouseOverRoof == value) return;

            isMouseOverRoof = value;
            RefreshDitherState();
        }

        public void AddPlayerInside()
        {
            playerInsideCount++;
            RefreshDitherState();
        }

        public void RemovePlayerInside()
        {
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
            RefreshDitherState();
        }

        private void RefreshDitherState()
        {
            float nextAlpha = ShouldDither ? targetAlpha : 1f;
            StartFade(nextAlpha);
        }

        private void StartFade(float target)
        {
            if(fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if(fadeDuration <= 0f)
            {
                currentAlpha = target;
                ApplyAlpha(currentAlpha);
                return;
            }

            fadeCoroutine = StartCoroutine(FadeCoroutine(target));
        }

        private IEnumerator FadeCoroutine(float target)
        {
            float start = currentAlpha;
            float elapsed = 0f;

            while(elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                currentAlpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                ApplyAlpha(currentAlpha);
                yield return null;
            }

            currentAlpha = target;
            ApplyAlpha(currentAlpha);
        }

        private void ApplyAlpha(float alpha)
        {
            foreach(Renderer roofRenderer in roofRenderers)
            {
                if(roofRenderer == null) continue;
                roofRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(RoofDitherAlpha, alpha);
                propertyBlock.SetFloat(MouseDitherObjectEnabled, 1f);
                roofRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
