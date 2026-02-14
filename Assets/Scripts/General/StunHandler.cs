using System.Collections;
using UnityEngine;

public class StunHandler : MonoBehaviour, IStunnable
{
    public bool IsStunned { get; private set; }

    public void Stun(float duration)
    {
        if (IsStunned) StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));

        EventManager.Publish(new CharacterStunnedEvent(this.gameObject, duration));
        Debug.Log($"{gameObject.name} is stunned for {duration} seconds.");
    }

    IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        yield return new WaitForSeconds(duration);
        IsStunned = false;
        Debug.Log($"{gameObject.name} is no longer stunned.");
    }
}