using UnityEngine;

public struct BurnFuelEvent : IGameEvent
{
    public PickableObject item;

    public BurnFuelEvent(PickableObject item)
    {
        this.item = item; 
    }
}
