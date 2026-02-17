public interface IFlammable
{
    // for item that can be ignited, e.g. crate and item on gird, etc.
    bool IsOnFire { get; }
    void Ignite();
    void Extinguish();
}