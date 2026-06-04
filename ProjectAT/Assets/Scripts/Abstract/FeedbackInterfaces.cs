using UnityEngine;

public interface IHoverableFeedback //아웃라인의 활성화 관련된 인터페이스. 만약 플레이어가 Target으로 선택하면 아웃라인이 켜짐.
{
    void OnHoverEnter();
    void OnHoverExit();
}

/// <summary>
/// 플레이어가 어떤 행위(상호작용, 스킬대상 지정 등)의 Target으로 선택했을 때의 피드백 관련 인터페이스.
/// </summary>
public interface ITargetableFeedback
{
    void OnTargeted();
    void OnUntargeted();
}


/// <summary>
/// 플레이어가 좌클릭을 할 경우 해당 Entity의 정보를 나타내게 해주는 인터페이스.
/// TODO : 추후에 체력바, 상태이상 아이콘을 띄워주는 로직 구현할때 이 인터페이스 사용하자.(26.06.04)
/// </summary>
public interface IStatusPresentable
{
    EntityStatus Target { get; }
    Transform StatusAnchor { get; }
}