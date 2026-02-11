using UnityEngine;

public class PickupHighlight : MonoBehaviour
{
    public Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();

        outline.enabled = false;
    }
    public void SetHighlight(bool value)
    {
        outline.enabled = value;
    }
}
