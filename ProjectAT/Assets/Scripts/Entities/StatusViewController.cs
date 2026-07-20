using System.Collections.Generic;
using ProjectAT.FieldUI;
using UnityEngine;

public class StatusViewController : MonoBehaviour
{
    [Header("Module References")]
    [SerializeField] private BuffModule buffModule;

    [Header("Target")]
    [SerializeField] private Transform targetAnchor;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.4f, 0f);
    [SerializeField] private Vector2 baseScreenOffset = Vector2.zero;

    [Header("Status View Pool")]
    [SerializeField] private RadialFillImageView radialFillImageViewPrefab;
    [SerializeField, Min(1)] private int poolSize = 6;
    [SerializeField] private Sprite fallbackIcon;

    [Header("Layout")]
    [SerializeField] private Vector2 iconSize = new Vector2(36f, 36f);
    [SerializeField] private float spacing = 4f;

    private readonly List<RadialFillImageView> viewPool = new List<RadialFillImageView>();
    private readonly Dictionary<BuffInstance, RadialFillImageView> activeViews = new Dictionary<BuffInstance, RadialFillImageView>();
    private readonly List<BuffInstance> orderedBuffs = new List<BuffInstance>();

    private void Awake()
    {
        ResolveReferences();
        CreatePool();
    }

    private void OnEnable()
    {
        if (buffModule == null)
        {
            return;
        }

        buffModule.OnBuffAdded += HandleBuffAdded;
        buffModule.OnBuffRemoved += HandleBuffRemoved;
        buffModule.OnBuffRefreshed += HandleBuffRefreshed;

        SyncActiveBuffs();
    }

    private void OnDisable()
    {
        if (buffModule != null)
        {
            buffModule.OnBuffAdded -= HandleBuffAdded;
            buffModule.OnBuffRemoved -= HandleBuffRemoved;
            buffModule.OnBuffRefreshed -= HandleBuffRefreshed;
        }

        ClearActiveViews();
    }

    private void OnDestroy()
    {
        DestroyPool();
    }

    private void ResolveReferences()
    {
        if (buffModule == null)
        {
            buffModule = GetComponentInParent<BuffModule>();
        }

        if (targetAnchor == null)
        {
            targetAnchor = transform;
        }
    }

    private void HandleBuffAdded(BuffInstance buff)
    {
        TryShowBuff(buff);
        LayoutActiveViews();
    }

    private void HandleBuffRemoved(BuffInstance buff)
    {
        if (!activeViews.TryGetValue(buff, out RadialFillImageView view))
        {
            return;
        }

        view.Unbind();
        view.gameObject.SetActive(false);

        activeViews.Remove(buff);
        orderedBuffs.Remove(buff);

        LayoutActiveViews();
    }

    private void HandleBuffRefreshed(BuffInstance buff)
    {
        if (!activeViews.TryGetValue(buff, out RadialFillImageView view))
        {
            TryShowBuff(buff);
            LayoutActiveViews();
            return;
        }

        view.Bind(buff, fallbackIcon);
    }

    private void SyncActiveBuffs()
    {
        if (buffModule == null || viewPool.Count == 0)
        {
            return;
        }

        ClearActiveViews();

        foreach (BuffInstance buff in buffModule.ActiveBuffs)
        {
            TryShowBuff(buff);
        }

        LayoutActiveViews();
    }

    private void TryShowBuff(BuffInstance buff)
    {
        if (buff == null || activeViews.ContainsKey(buff))
        {
            return;
        }

        RadialFillImageView view = GetInactiveView();

        if (view == null)
        {
            Debug.LogWarning($"[{nameof(StatusViewController)}] Status view pool is full.", this);
            return;
        }

        view.gameObject.SetActive(true);
        view.Bind(buff, fallbackIcon);

        activeViews.Add(buff, view);
        orderedBuffs.Add(buff);
    }

    private RadialFillImageView GetInactiveView()
    {
        for (int i = 0; i < viewPool.Count; i++)
        {
            if (!viewPool[i].gameObject.activeSelf && !viewPool[i].IsBound)
            {
                return viewPool[i];
            }
        }

        return null;
    }

    private void CreatePool()
    {
        if (radialFillImageViewPrefab == null)
        {
            Debug.LogError($"[{nameof(StatusViewController)}] RadialFillImageView prefab is not assigned.", this);
            return;
        }

        for (int i = viewPool.Count; i < poolSize; i++)
        {
            RadialFillImageView view = Instantiate(radialFillImageViewPrefab, FieldUI.OverlayRoot, false);
            view.name = $"{gameObject.name}_StatusView_{i}";
            view.ConfigureFollowTarget(targetAnchor, worldOffset, baseScreenOffset);
            view.Unbind();
            view.gameObject.SetActive(false);

            RectTransform rectTransform = view.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = iconSize;
            }

            viewPool.Add(view);
        }
    }

    private void DestroyPool()
    {
        for (int i = 0; i < viewPool.Count; i++)
        {
            if (viewPool[i] != null)
            {
                Destroy(viewPool[i].gameObject);
            }
        }

        viewPool.Clear();
        activeViews.Clear();
        orderedBuffs.Clear();
    }

    private void ClearActiveViews()
    {
        foreach (RadialFillImageView view in activeViews.Values)
        {
            view.Unbind();
            view.gameObject.SetActive(false);
        }

        activeViews.Clear();
        orderedBuffs.Clear();
    }

    private void LayoutActiveViews()
    {
        int count = orderedBuffs.Count;

        if (count <= 0)
        {
            return;
        }

        float totalWidth = count * iconSize.x + (count - 1) * spacing;
        float startX = -totalWidth * 0.5f + iconSize.x * 0.5f;

        for (int i = 0; i < count; i++)
        {
            if (!activeViews.TryGetValue(orderedBuffs[i], out RadialFillImageView view))
            {
                continue;
            }

            RectTransform rectTransform = view.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = iconSize;
                rectTransform.SetSiblingIndex(i);
            }

            Vector2 layoutOffset = new Vector2(startX + i * (iconSize.x + spacing), 0f);
            view.ConfigureFollowTarget(targetAnchor, worldOffset, baseScreenOffset + layoutOffset);
        }
    }
}
