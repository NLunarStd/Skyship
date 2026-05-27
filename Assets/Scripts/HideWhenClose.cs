using UnityEngine;

public class HideWhenClose : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ระยะความห่าง ถ้าน้อยกว่านี้ใบเรือจะล่องหน")]
    public float hideDistance = 15f; 
    
    private Renderer[] meshRenderers;
    private Camera mainCamera;
    private bool isHidden = false;

    void Start()
    {
        // ดึง Renderer ทั้งของตัวเองและลูกๆ ทั้งหมด (เผื่อใบเรือมีหลายชิ้น)
        meshRenderers = GetComponentsInChildren<Renderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) 
            {
                Debug.LogWarning("HideWhenClose: ไม่พบ Camera ที่มี Tag ว่า MainCamera");
                return;
            }
        }

        // วัดระยะห่างจากกล้อง (แบบไม่สนความสูงแกน Y เพื่อแก้ปัญหาจุดศูนย์กลางใบเรืออยู่สูงเกินไป)
        Vector3 myPos = transform.position;
        myPos.y = 0;
        
        Vector3 camPos = mainCamera.transform.position;
        camPos.y = 0;

        float distance = Vector3.Distance(myPos, camPos);

        // ถ้าเข้าใกล้เกินไป
        if (distance < hideDistance)
        {
            if (!isHidden)
            {
                SetRenderers(false);
                isHidden = true;
            }
        }
        else // ถ้าอยู่ไกล
        {
            if (isHidden)
            {
                SetRenderers(true);
                isHidden = false;
            }
        }
    }

    private void SetRenderers(bool state)
    {
        foreach (var r in meshRenderers)
        {
            if (r != null) r.enabled = state;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hideDistance);
    }
}
