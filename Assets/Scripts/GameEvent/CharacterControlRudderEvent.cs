using UnityEngine;

public struct CharacterControlRudderEvent : IGameEvent
{
    public bool value;

    public CharacterControlRudderEvent(bool value)
    {
        this.value = value;
    }
}
