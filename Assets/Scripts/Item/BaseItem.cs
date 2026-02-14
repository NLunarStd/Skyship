using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseItem : MonoBehaviour, IPickable, INetworkSync
{
    [Header("Base Settings")]
    public GameEnum.Faction itemFaction = GameEnum.Faction.Neutral;
    protected Rigidbody rb;
    protected bool isBeingHeld;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void PickUp()
    {
        isBeingHeld = true;
    }

    public virtual void Drop()
    {
        isBeingHeld = false;
    }

    public abstract void Sync(); 
}