using UnityEngine;

public enum OrderKind
{
    Attack,
    Search,
    Patrol,
    Disengage
}

public readonly struct SquadOrder
{
    public readonly OrderKind OrderKind;
    public readonly IPerceivable Target;       // Attack에서 사용
    public readonly Vector3 Position;          // Attack 슬롯 / Search 지점 / Patrol 웨이포인트

    private SquadOrder(OrderKind kind, IPerceivable target, Vector3 pos)
    {
        OrderKind = kind;
        Target = target;
        Position = pos;
    }

    public static SquadOrder Attack(IPerceivable target, Vector3 slot) => new SquadOrder(OrderKind.Attack, target, slot);

    public static SquadOrder Search(Vector3 point) => new SquadOrder(OrderKind.Search, null, point);

    public static SquadOrder Patrol(Vector3 waypoint) => new SquadOrder(OrderKind.Patrol, null, waypoint);

    public static SquadOrder Disengage() => new SquadOrder(OrderKind.Disengage, null, Vector3.zero);
}
