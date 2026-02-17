using UnityEngine;

public class DeckCulling : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask lowerDeckLayer;
    public LayerMask upperDeckLayer;
    public LayerMask topDeckLayer;

    private void OnEnable()
    {
        EventManager.Subscribe<CullTopDeckEvent>(OnCullTopDeck);
        EventManager.Subscribe<CullUpperDeckEvent>(OnCullUpperDeck);
        EventManager.Subscribe<CullLowerDeckEvent>(OnCullLowerDeck);


    }

    void OnDisable()
    {
        EventManager.UnSubscribe<CullTopDeckEvent>(OnCullTopDeck);
        EventManager.UnSubscribe<CullUpperDeckEvent>(OnCullUpperDeck);
        EventManager.UnSubscribe<CullLowerDeckEvent>(OnCullLowerDeck);
    }

    void OnCullTopDeck(CullTopDeckEvent e)
    {
        if (e.CullTopDeck)
        {
            mainCamera.cullingMask &= ~(topDeckLayer);
        }
        else
        {
            mainCamera.cullingMask |= topDeckLayer;
        }
    }

    void OnCullUpperDeck(CullUpperDeckEvent e)
    {
        if (e.CullUpperDeck)
        {
            mainCamera.cullingMask &= ~(upperDeckLayer);
        }
        else
        {
            mainCamera.cullingMask |= upperDeckLayer;
        }
    }

    void OnCullLowerDeck(CullLowerDeckEvent e)
    {
        if (e.CullLowerDeck)
        {
            mainCamera.cullingMask &= ~(lowerDeckLayer);
        }
        else
        {
            mainCamera.cullingMask |= lowerDeckLayer;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        mainCamera.cullingMask &= ~(upperDeckLayer);
    //        Debug.Log("Culling mask updated to exclude upper deck layer.");
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        mainCamera.cullingMask |= upperDeckLayer;
    //        Debug.Log("Culling mask updated to include upper deck layer.");
    //    }
    //}
}