using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCore : MonoBehaviour
{
    private Dictionary<PickableObject, Coroutine> activeBurns
    = new Dictionary<PickableObject, Coroutine>();

    private void OnEnable()
    {
        EventManager.Subscribe<BurnFuelEvent>(StartBurnObject);
        EventManager.Subscribe<CancelBurnEvent>(HandleCancelBurn);
    }

    private void OnDisable()
    {
        EventManager.UnSubscribe<BurnFuelEvent>(StartBurnObject);
        EventManager.UnSubscribe<CancelBurnEvent>(HandleCancelBurn);
    }

    void StartBurnObject(BurnFuelEvent e)
    {
        if (activeBurns.ContainsKey(e.item))
            return;

        Coroutine c = StartCoroutine(
            BurnObject(e.item)
        );

        activeBurns.Add(e.item, c);
    }

    IEnumerator BurnObject(PickableObject item)
    {
        float t = item.burnTime;
        float fuel = item.fuelAmount;

        while (t > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        AddFuel(fuel);

        activeBurns.Remove(item);
        Destroy(item.gameObject);

    }

    void AddFuel(float amount)
    {
        EventManager.Publish(new ShipGetFuel(amount));
    }

    void HandleCancelBurn(CancelBurnEvent e)
    {
        CancelBurn(e.item);
    }
    public void CancelBurn(PickableObject item)
    {
        if (activeBurns.TryGetValue(item, out Coroutine c))
        {
            StopCoroutine(c);
            activeBurns.Remove(item);
            Debug.Log("Cancel burn item: "+ item);
        }
    }

}
