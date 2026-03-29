using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    
    
    public void DisableOnContact()
    {
        Destroy(this.gameObject);
    }
}
