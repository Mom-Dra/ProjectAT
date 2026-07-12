using UnityEngine;
using UnityEngine.UI;

namespace ProjectAT.FieldUI
{
    public abstract class FieldUI : MonoBehaviour
    {
        private static RectTransform overlayRoot;

        [Header("Field UI View")]
        [SerializeField] private RectTransform viewPrefab;

        [Header("Field UI Target")]
        [SerializeField] protected Transform targetAnchor;
        [SerializeField] protected Vector3 worldOffset = Vector3.up;
        [SerializeField] protected Vector2 screenOffset = Vector2.zero;
        [SerializeField] private bool hideWhenBehindCamera = true;

        private RectTransform viewInstance;
        private Camera mainCam;

        protected RectTransform ViewInstance => viewInstance;
        protected Camera MainCam => mainCam;

        protected virtual void Awake()
        {
            ResolveReferences();

            mainCam = Camera.main;

            if (!TryCreateView())
            {
                enabled = false;
                return;
            }

            BindView(viewInstance);
            SetVisible(false);
        }

        protected virtual void OnDestroy()
        {
            if (viewInstance != null)
            {
                Destroy(viewInstance.gameObject);
                viewInstance = null;
            }
        }

        protected virtual void LateUpdate()
        {
            if (!ShouldShow() || !FollowTarget())
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            RefreshContent();
        }

        protected virtual void ResolveReferences()
        {
            if (targetAnchor == null)
            {
                targetAnchor = transform;
            }
        }

        protected abstract bool ShouldShow();

        // 생성된 View 프리팹에서 필요한 컴포넌트를 연결한다.
        protected abstract void BindView(RectTransform view);

        protected abstract void RefreshContent();

        protected void SetVisible(bool visible)
        {
            if (viewInstance != null &&
                viewInstance.gameObject.activeSelf != visible)
            {
                viewInstance.gameObject.SetActive(visible);
            }
        }

        private bool TryCreateView()
        {
            if (viewPrefab == null)
            {
                Debug.LogError($"[{GetType().Name}] View Prefab이 지정되지 않았습니다.", this);
                return false;
            }

            viewInstance = Instantiate(viewPrefab, GetOverlayRoot(), false);

            viewInstance.name = $"{gameObject.name}_{GetType().Name}";
            return true;
        }

        private bool FollowTarget()
        {
            if (targetAnchor == null || viewInstance == null)
            {
                return false;
            }

            if (mainCam == null)
            {
                mainCam = Camera.main;

                if (mainCam == null)
                {
                    return false;
                }
            }

            Vector3 screenPosition = mainCam.WorldToScreenPoint(targetAnchor.position + worldOffset);
            if (hideWhenBehindCamera && screenPosition.z <= 0f)
            {
                return false;
            }

            viewInstance.position = new Vector3(screenPosition.x + screenOffset.x, screenPosition.y + screenOffset.y, 0f);
            return true;
        }

        private static RectTransform GetOverlayRoot()
        {
            if (overlayRoot != null)
            {
                return overlayRoot;
            }

            // 필드 UI 오버레이를 위한 Canvas 생성 로직
            GameObject canvasObject = new GameObject("FieldOverlayCanvas", typeof(Canvas), typeof(CanvasScaler));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            overlayRoot = canvasObject.GetComponent<RectTransform>();
            return overlayRoot;
        }
    }
}