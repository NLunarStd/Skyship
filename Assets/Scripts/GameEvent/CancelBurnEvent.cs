using UnityEngine;

public struct CancelBurnEvent : IGameEvent
{
    public PickableObject item;

    public CancelBurnEvent(PickableObject item)
    {
        this.item = item;
    }
}
