using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 2f;

    private GameObject carriedObject;

    private GameObject currentHighlight;

    // Throwing
    float holdTime;
    bool isHoldingKey;
    [SerializeField]
    Transform throwDir;

    public float chargeStart = 0.5f;   // น้อยกว่านี้ = วาง
    public float chargeTime = 1.5f;    // เต็มหลอด
    public float throwForce = 10f;     // แรงสูงสุด

    [Header("UI")]
    public Image chargeFill;
    public GameObject chargeUI;

    [Header("Input Action Reference")]
    public InputActionReference pickupAction;

    private void Start()
    {
        chargeUI.SetActive(false);
    }
    void Update()
    {
        if (carriedObject == null)
        {
            UpdateHighlight();

            if (pickupAction.action.WasPerformedThisFrame()) // Get key down
                TryPickup();
        }
        else
        {
            // เริ่มกด
            if (pickupAction.action.WasPerformedThisFrame())
            {
                isHoldingKey = true;
                holdTime = 0f;
                chargeUI.gameObject.SetActive(true);
                chargeFill.fillAmount = 0f;
            }

            // กดค้าง
            if (pickupAction.action.IsPressed() && isHoldingKey)
            {
                holdTime += Time.deltaTime;

                if (holdTime > chargeStart)
                {
                    float percent = Mathf.Clamp01((holdTime - chargeStart) / chargeTime);
                    // ?? เอา percent ไปอัปเดต UI หลอดได้ตรงนี้
                    chargeFill.fillAmount = percent;
                }
            }

            // ปล่อยปุ่ม
            if (pickupAction.action.WasReleasedThisFrame() && isHoldingKey)
            {
                isHoldingKey = false;

                if (holdTime < chargeStart)
                {
                    Drop();
                    chargeUI.SetActive(false);
                    Debug.Log("Player dropped an item");
                }
                else
                {
                    Throw();
                    chargeUI.SetActive(false);
                    Debug.Log("Player Throwed an item");
                }
            }
        }

    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        float closestDistance = Mathf.Infinity;
        GameObject closestObject = null;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Pickup")) continue;

            Vector3 dirToObject = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToObject);

            if (dot < 0.5f) continue; // only object that is in front

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = hit.gameObject;
            }
        }

        // ถ้ามีของที่ใกล้สุด
        if (closestObject != null)
        {
            carriedObject = closestObject;

            PickableObject pickable = carriedObject.GetComponent<PickableObject>();
            if (pickable != null)
            {
                pickable.RemoveFromFurnace();
            }

            Rigidbody rb = carriedObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            carriedObject.transform.position = holdPoint.position;
            carriedObject.transform.SetParent(holdPoint);
            carriedObject.transform.rotation = Quaternion.identity;

            Collider col = carriedObject.GetComponent<Collider>();
            col.enabled = false;

            SoundManager.instance.PlayPickupSound();

        }
    }

    void Drop()
    {
        Rigidbody rb = carriedObject.GetComponent<Rigidbody>();

        carriedObject.transform.SetParent(null);
        rb.isKinematic = false;   // เปิด physics กลับ

        Collider col = carriedObject.GetComponent<Collider>();
        col.enabled = true;

        carriedObject = null;
        SoundManager.instance.PlayDropSound();
    }

    void UpdateHighlight()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        float closestDistance = Mathf.Infinity;
        GameObject closestObject = null;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Pickup")) continue;

            Vector3 dirToObject = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToObject);

            if (dot < 0.5f) continue; // only object that is in front

            float distance = Vector3.Distance(holdPoint.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = hit.gameObject;
            }
        }

        // ถ้าเปลี่ยนตัว
        if (currentHighlight != closestObject)
        {
            // ปิดอันเก่า
            if (currentHighlight != null)
                currentHighlight.GetComponent<PickupHighlight>().SetHighlight(false);

            currentHighlight = closestObject;

            // เปิดอันใหม่
            if (currentHighlight != null)
                currentHighlight.GetComponent<PickupHighlight>().SetHighlight(true);
        }
    }

    void Throw()
    {
        Rigidbody rb = carriedObject.GetComponent<Rigidbody>();

        carriedObject.transform.SetParent(null);

        rb.isKinematic = false;
        carriedObject.GetComponent<Collider>().enabled = true;

        float percent = Mathf.Clamp01((holdTime - chargeStart) / chargeTime);
        float force = percent * throwForce;
        Debug.Log("Throw force = " + percent);

        // ขว้างไปข้างหน้าของกล้อง
        Vector3 dir = throwDir.forward;

        rb.AddForce(dir * force, ForceMode.Impulse);

        carriedObject = null;

        SoundManager.instance.PlayThrowSound();

    }



}

