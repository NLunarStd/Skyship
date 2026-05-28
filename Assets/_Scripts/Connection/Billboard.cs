using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null || !cam.gameObject.activeInHierarchy)
        {
            cam = Camera.main;
        }

        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }
}
