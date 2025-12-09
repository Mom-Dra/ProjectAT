using UnityEngine;
using UnityEngine.UI; // Slider나 Image를 사용하려면 필요

public class EnemyUIFollower : MonoBehaviour
{
    // 이 UI가 따라다닐 적의 Transform
    public Transform target;

    // 화면에 표시될 UI (예: Slider 프리팹을 인스턴스화한 것)
    public RectTransform uiRect;

    // 머리 위로 띄울 높이 오프셋
    public Vector3 offset = new Vector3(0, 2f, 0);

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // 만약 UI가 프리팹이라면 여기서 생성(Instantiate)하고
        // uiRect 변수에 할당해줘야 합니다.
        // 예: GameObject uiInstance = Instantiate(uiPrefab, canvasTransform);
        //      uiRect = uiInstance.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        //if (target == null || uiRect == null) return;

        //// 1. 적의 3D 월드 좌표(머리 위)를 2D 스크린 좌표로 변환합니다.
        //Vector3 worldPosition = target.position + offset;
        //Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        //// 2. 변환된 2D 스크린 좌표를 UI의 위치로 설정합니다.
        //uiRect.position = screenPosition;

        //// (선택 사항) 적이 카메라 뒤에 있거나 너무 멀면 UI를 숨깁니다.
        //bool isBehindCamera = screenPosition.z < 0;
        //uiRect.gameObject.SetActive(!isBehindCamera);

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}