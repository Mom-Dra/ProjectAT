public interface IBushHideable
{
    bool IsHidden { get; }

    void EnterBush(Bush bush);
    void ExitBush(Bush bush);
}
