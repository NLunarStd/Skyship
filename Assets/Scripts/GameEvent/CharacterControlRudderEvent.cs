using UnityEngine;

public struct CharacterControlRudderEvent : IGameEvent
{
    public bool value;
    public NetworkShipHandler ship;

    public CharacterControlRudderEvent(bool value, NetworkShipHandler ship)
    {
        this.value = value;
        this.ship = ship;
    }
}
