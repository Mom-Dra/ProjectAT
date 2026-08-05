using UnityEngine;
using UnityEngine.UI;

namespace ProjectAT.FieldUI
{
    public enum FieldUIFollowMode : ushort {None, ScreenSpaceOverlay, }
    public abstract class FieldUI : MonoBehaviour
    {
        private static RectTransform overlayRoot;

        [Header("Field UI View")]
        [SerializeField] private RectTransform viewPrefab;
        [SerializeField] private FieldUIFollowMode followMode = FieldUIFollowMode.ScreenSpaceOverlay;

        [Header("Field UI Target")]
        [SerializeField] protected Transform targetAnchor;
        [SerializeField] protected Vector3 worldOffset = Vector3.up;
        [SerializeField] protected Vector2 screenOffset = Vector2.zero;
        [SerializeField] private bool hideWhenBehindCamera = true;

        private RectTransform viewInstance; //자신을 어느 Canvas나 오브젝트에 표시할지
        private Camera mainCam;
        private bool ownsViewInstance;

        public static RectTransform OverlayRoot => GetOverlayRoot();

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
            if (ownsViewInstance && viewInstance != null)
            {
                Destroy(viewInstance.gameObject);
            }

            viewInstance = null;
            ownsViewInstance = false;
        }

        protected virtual void LateUpdate()
        {
            if (!ShouldShow())
            {
                SetVisible(false);
                return;
            }

            if (followMode == FieldUIFollowMode.ScreenSpaceOverlay && !FollowTarget())
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

        public void ConfigureFollowTarget(Transform target, Vector3 worldOffset, Vector2 screenOffset)
        {
            targetAnchor = target != null ? target : transform;
            this.worldOffset = worldOffset;
            this.screenOffset = screenOffset;
        }

        public void SetScreenOffset(Vector2 screenOffset)
        {
            this.screenOffset = screenOffset;
        }

        public void SetFollowMode(FieldUIFollowMode mode)
        {
            followMode = mode;
        }

        protected abstract bool ShouldShow();

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
                viewInstance = transform as RectTransform;
                ownsViewInstance = false;

                if (viewInstance == null)
                {
                    Debug.LogError($"[{GetType().Name}] View Prefab is not assigned and this object has no RectTransform.", this);
                    return false;
                }

                return true;
            }

            viewInstance = Instantiate(viewPrefab, OverlayRoot, false);
            viewInstance.name = $"{gameObject.name}_{GetType().Name}";
            ownsViewInstance = true;
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

            Canvas canvas = InGameManager.Instance?.UIManager.FieldOverlayCanvas;

            if(canvas != null)
            {
                overlayRoot = canvas.GetComponent<RectTransform>();
                if (overlayRoot != null)
                {
                    return overlayRoot;
                }
            }

            GameObject canvasObject = new GameObject("FieldOverlayCanvas", typeof(Canvas), typeof(CanvasScaler));

            canvas = canvasObject.GetComponent<Canvas>();
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
