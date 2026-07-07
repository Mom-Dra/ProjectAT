using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectScreenUI : MonoBehaviour
{
    private static RectTransform overlayRoot;

    [Header("References")]
    [SerializeField] private CrowdControlModule crowdControlModule;
    [SerializeField] private BuffModule buffModule;
    [SerializeField] private Transform targetAnchor;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.4f, 0f);
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    [Header("Visual")]
    [SerializeField] private Sprite fallbackStunIcon;
    [SerializeField] private Vector2 iconSize = new Vector2(44f, 44f);
    [SerializeField] private bool showRemainingTime = true;

    private RectTransform rootRect;
    private Image iconImage;
    private TextMeshProUGUI fallbackLabel;
    private TextMeshProUGUI remainingTimeText;
    private Camera mainCamera;
    private bool isShowingStun;

    private void Awake()
    {
        crowdControlModule = crowdControlModule != null ? crowdControlModule : GetComponent<CrowdControlModule>();
        buffModule = buffModule != null ? buffModule : GetComponent<BuffModule>();
        targetAnchor = targetAnchor != null ? targetAnchor : transform;
        mainCamera = Camera.main;

        CreateUI();
        SetVisible(false);
    }

    private void OnEnable()
    {
        if (crowdControlModule != null)
        {
            crowdControlModule.OnStunStarted += ShowStun;
            crowdControlModule.OnStunEnded += HideStun;

            if (crowdControlModule.IsStunned)
            {
                ShowStun();
            }
        }

        if (buffModule != null)
        {
            buffModule.OnBuffsChanged += RefreshStunView;
        }
    }

    private void OnDisable()
    {
        if (crowdControlModule != null)
        {
            crowdControlModule.OnStunStarted -= ShowStun;
            crowdControlModule.OnStunEnded -= HideStun;
        }

        if (buffModule != null)
        {
            buffModule.OnBuffsChanged -= RefreshStunView;
        }

        SetVisible(false);
    }

    private void LateUpdate()
    {
        if (!isShowingStun)
        {
            return;
        }

        FollowTarget();
        RefreshRemainingTime();
    }

    private void OnDestroy()
    {
        if (rootRect != null)
        {
            Destroy(rootRect.gameObject);
        }
    }

    private void ShowStun()
    {
        isShowingStun = true;
        RefreshStunView();
        SetVisible(true);
    }

    private void HideStun()
    {
        isShowingStun = false;
        SetVisible(false);
    }

    private void RefreshStunView()
    {
        BuffInstance stunBuff = FindVisibleStunBuff();
        Sprite icon = stunBuff != null ? stunBuff.Icon : null;

        if (iconImage != null)
        {
            iconImage.sprite = icon != null ? icon : fallbackStunIcon;
            iconImage.enabled = iconImage.sprite != null;
        }

        if (fallbackLabel != null)
        {
            fallbackLabel.enabled = iconImage == null || !iconImage.enabled;
        }

        RefreshRemainingTime();
    }

    private void RefreshRemainingTime()
    {
        if (remainingTimeText == null)
        {
            return;
        }

        BuffInstance stunBuff = FindVisibleStunBuff();
        bool shouldShowTime = showRemainingTime && stunBuff != null;
        remainingTimeText.enabled = shouldShowTime;

        if (!shouldShowTime)
        {
            return;
        }

        remainingTimeText.text = stunBuff.IsPermanent
            ? "\u221E"
            : Mathf.CeilToInt(stunBuff.RemainingTime).ToString();
    }

    private BuffInstance FindVisibleStunBuff()
    {
        if (buffModule == null)
        {
            return null;
        }

        BuffInstance bestBuff = null;

        foreach (BuffInstance buff in buffModule.ActiveBuffs)
        {
            if (buff.BuffData is not StunBuffData)
            {
                continue;
            }

            if (bestBuff == null || buff.IsPermanent || buff.RemainingTime > bestBuff.RemainingTime)
            {
                bestBuff = buff;
            }
        }

        return bestBuff;
    }

    private void FollowTarget()
    {
        if (targetAnchor == null || rootRect == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetAnchor.position + worldOffset);
        bool isInFrontOfCamera = screenPosition.z > 0f;
        rootRect.gameObject.SetActive(isInFrontOfCamera);

        if (!isInFrontOfCamera)
        {
            return;
        }

        rootRect.position = new Vector3(
            screenPosition.x + screenOffset.x,
            screenPosition.y + screenOffset.y,
            0f);
    }

    private void SetVisible(bool isVisible)
    {
        if (rootRect != null)
        {
            rootRect.gameObject.SetActive(isVisible);
        }
    }

    private void CreateUI()
    {
        RectTransform parent = GetOverlayRoot();

        GameObject rootObject = new GameObject($"{gameObject.name}_StatusEffectUI", typeof(RectTransform), typeof(CanvasGroup));
        rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.sizeDelta = new Vector2(iconSize.x, iconSize.y + 18f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = CreateImage(rootRect, "Background");
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = new Vector2(0.5f, 1f);
        backgroundRect.anchorMax = new Vector2(0.5f, 1f);
        backgroundRect.pivot = new Vector2(0.5f, 1f);
        backgroundRect.sizeDelta = iconSize;
        backgroundRect.anchoredPosition = Vector2.zero;
        background.color = new Color(0f, 0f, 0f, 0.58f);

        iconImage = CreateImage(backgroundRect, "StunIcon");
        RectTransform iconRect = iconImage.rectTransform;
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(5f, 5f);
        iconRect.offsetMax = new Vector2(-5f, -5f);
        iconImage.preserveAspect = true;

        fallbackLabel = CreateText(backgroundRect, "FallbackLabel", "!");
        fallbackLabel.fontSize = 32f;
        fallbackLabel.alignment = TextAlignmentOptions.Center;
        fallbackLabel.color = new Color(1f, 0.9f, 0.25f, 1f);

        remainingTimeText = CreateText(rootRect, "RemainingTimeText", string.Empty);
        RectTransform timeRect = remainingTimeText.rectTransform;
        timeRect.anchorMin = new Vector2(0.5f, 0f);
        timeRect.anchorMax = new Vector2(0.5f, 0f);
        timeRect.pivot = new Vector2(0.5f, 0f);
        timeRect.sizeDelta = new Vector2(60f, 18f);
        timeRect.anchoredPosition = Vector2.zero;
        remainingTimeText.fontSize = 15f;
        remainingTimeText.alignment = TextAlignmentOptions.Center;
        remainingTimeText.color = Color.white;
    }

    private static RectTransform GetOverlayRoot()
    {
        if (overlayRoot != null)
        {
            return overlayRoot;
        }

        GameObject canvasObject = new GameObject("StatusEffectOverlayCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        overlayRoot = canvasObject.GetComponent<RectTransform>();
        return overlayRoot;
    }

    private static Image CreateImage(Transform parent, string objectName)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string objectName, string text)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI textUI = textObject.GetComponent<TextMeshProUGUI>();
        textUI.text = text;
        textUI.raycastTarget = false;

        RectTransform rectTransform = textUI.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return textUI;
    }
}
