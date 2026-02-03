using UnityEngine;

public class EnemyUIModule : MonoBehaviour, IInteractable
{
    // 이 UI가 따라다닐 적의 Transform
    public Transform target;

    // 화면에 표시될 UI (예: Slider 프리팹을 인스턴스화한 것)
    public RectTransform uiRect;

    // 머리 위로 띄울 높이 오프셋
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 worldPosition = transform.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        uiRect.position = screenPosition;
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }

    public void OnInteract()
    {

    }
}