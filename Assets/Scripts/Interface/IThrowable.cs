using UnityEngine;
public interface IThrowable
{
    // for item that can be thrown by player, and can cause damage, knockback or stun to enemy when it hit the enemy
    void Throw(Vector3 direction, float throwForce);
}
