using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform playerCharacter;
    public Camera mainCamera;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    private void LateUpdate()
    {
        getCurrentPlayerPosition();
    }

    void getCurrentPlayerPosition()
    {
        if (playerCharacter == null)
            return;

        mainCamera = Camera.main;

        Vector3 playerPos = playerCharacter.position;

        Vector3 cameraPos = mainCamera.transform.position;

        Vector3 dir = playerPos - cameraPos;

        RaycastHit hit;

        if (Physics.Raycast(cameraPos, dir, out hit))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("DeckTop"))
            {
                EventManager.Publish(new CullTopDeckEvent
                {
                    CullTopDeck = true
                });
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("DeckUpper"))
            {
                EventManager.Publish(new CullUpperDeckEvent
                {
                    CullUpperDeck = true
                });
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("DeckLower"))
            {
                EventManager.Publish(new CullLowerDeckEvent
                {
                    CullLowerDeck = true
                });
            }
            else
            {
                EventManager.Publish(new CullTopDeckEvent
                {
                    CullTopDeck = false
                });
                EventManager.Publish(new CullUpperDeckEvent
                {
                    CullUpperDeck = false
                });
                EventManager.Publish(new CullLowerDeckEvent
                {
                    CullLowerDeck = false
                });
            }


        }
    }
}
