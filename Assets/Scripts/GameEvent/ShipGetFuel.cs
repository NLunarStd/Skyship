using UnityEngine;

public struct ShipGetFuel : IGameEvent
{
    public float fuelAmount;

    public ShipGetFuel(float fuelAmount)
    {
        this.fuelAmount = fuelAmount;

    }
}
    

