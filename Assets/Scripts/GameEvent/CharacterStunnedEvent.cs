using UnityEngine;

public struct CharacterStunnedEvent : IGameEvent
{
    public GameObject Victim;
    public float Duration;

    public CharacterStunnedEvent(GameObject victim, float duration)
    {
        Victim = victim;
        Duration = duration;
    }
}