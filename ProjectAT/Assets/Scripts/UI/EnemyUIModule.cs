using UnityEngine;

public class EnemyUIModule : MonoBehaviour, IUIHoverable
{
    // �� UI�� ����ٴ� ���� Transform
    public Transform target;

    // ȭ�鿡 ǥ�õ� UI (��: Slider �������� �ν��Ͻ�ȭ�� ��)
    public RectTransform uiRect;

    // �Ӹ� ���� ��� ���� ������
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    private Camera mainCamera;

    private void Start()
    {
        //mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        //Vector3 worldPosition = transform.position + offset;
        //Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        //uiRect.position = screenPosition;
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}