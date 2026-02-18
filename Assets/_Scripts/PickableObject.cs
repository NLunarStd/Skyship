using UnityEngine;

public class PickableObject : MonoBehaviour, IFuelResource
{
    public float fuelAmount;
    public float burnTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShipCore"))
        {
            TurnIntoFuel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ShipCore"))
        {
            Debug.Log("Exit ShipCore Trigger");
            RemoveFromFurnace();
        }
    }
    public void TurnIntoFuel()
    {
        EventManager.Publish(new BurnFuelEvent(this));
    }

    public void RemoveFromFurnace()
    {
        EventManager.Publish(new CancelBurnEvent(this));
    }
}
