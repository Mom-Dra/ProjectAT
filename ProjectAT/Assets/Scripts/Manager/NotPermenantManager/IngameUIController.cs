using UnityEngine;

public class IngameUIController : MonoBehaviour //IngameUIManager
{
    /* 
    [Notice]
    Ingame 씬에서 존재하는 UI 매니저같은 것임.
    Scene이 로드되면 UIManager에서 해당 Controller를 캐싱, 다른 오브젝트들이 UI를 띄워야할 일이 있을때(ex. 수류탄 시간초 표시, 상태이상 표시 등) 해당 Controller를 통해 UI를 띄움.
    정확힌 이 IngameUIContoller가 자신의 자식 오브젝트인 OverlayCanvas에다 위의 UI들을 띄우는 방식임.
    이것들을 구현해야함.
    관련 클래스 : FieldUI, GrenadeCountDownUI, StatusEffectScreenUI, UIManager, PlayerHUD
    */
}