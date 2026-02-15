using UnityEngine;

public class ShipController : MonoBehaviour
{
    public  bool EngineOn;

    public float moveSpeed = 5f;

    [Header("Fuel")]
    public float fuel = 100f;
    public float fuelDrainSpd = 1f;
    public float speedPercent;
    private float maxfuel;

    [Header("Rudder")]
    public float rudderAngle = 0f;        // มุมปัจจุบัน
    public float maxRudder = 45f;        // เอียงได้สุด
    public float turnSpeed = 20f;        // เรือหมุนแรงแค่ไหน

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        maxfuel = fuel;
    }
    private void Update()
    {
        //ForTestOnly
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (EngineOn)
                EngineOn = false;
            else
                EngineOn = true;
        }
    }


    void FixedUpdate()
    {
        if (EngineOn)
        {
            SailShip();
            DrainFuel();
        }
    }

    void SailShip()
    {
        speedPercent = GetSpeedPercent();
        // ===== เดินหน้า =====
        Vector3 move = transform.forward * moveSpeed *speedPercent* Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // ===== หมุน =====
        float turn = rudderAngle / maxRudder;
        Quaternion rot = Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f);

        rb.MoveRotation(rb.rotation * rot);
    }

    void DrainFuel()
    {
        float engineLoad = 1f;

        // เลี้ยวแรง = เพิ่มโหลด
        engineLoad += Mathf.Abs(rudderAngle) / maxRudder;

        fuel -= fuelDrainSpd * engineLoad * Time.fixedDeltaTime;

        if (fuel <= 0f)
        {
            fuel = 0f;
            EngineOn = false;
        }
    }

    float GetSpeedPercent()
    {
        float percent = fuel / maxfuel; // สมมติ max = 100

        if (percent <= 0f)
            return 0f;

        if (percent >= 0.1f)
            return 1f;

        // ช่วง 0 - 10%
        float t = percent / 0.1f;     // แปลงเป็น 0 ? 1
        return Mathf.Lerp(0.2f, 1f, t);
    }


}
