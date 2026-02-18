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
    private void OnEnable()
    {
        EventManager.Subscribe<ShipGetFuel>(IncreaseFuel);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<ShipGetFuel>(IncreaseFuel);
    }
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
        Vector3 move = transform.forward * moveSpeed * speedPercent * Time.fixedDeltaTime;
        transform.position += move;

        // ===== หมุน =====
        float turn = rudderAngle / maxRudder;
        float rotation = turn * turnSpeed * Time.fixedDeltaTime;

        transform.Rotate(0f, rotation, 0f);
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
            //EngineOn = false;
        }
    }

    float GetSpeedPercent()
    {
        float percent = fuel / maxfuel; // สมมติ max = 100

        if (percent <= 0f)
            return 0.2f;   // even fuel is 0, the ship will still sail with 20% speed

        if (percent >= 0.1f)
            return 1f;

        // ช่วง 0 - 10%
        float t = percent / 0.1f;     // แปลงเป็น 0 ? 1
        return Mathf.Lerp(0.2f, 1f, t);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.transform.SetParent(transform, true);  
        }
        if (other.CompareTag("Pickup"))
        {
            other.gameObject.transform.SetParent(transform, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }

        if (other.CompareTag("Pickup"))
        {
            other.transform.SetParent(null);
        }
    }

    void IncreaseFuel(ShipGetFuel e)
    {
        fuel += e.fuelAmount;
        if (fuel > 100f)
            fuel = 100f;
        Debug.Log("Added Fuel to the ship: + " + e.fuelAmount+
            " Now Fuel is at: "+fuel) ;
    }

}
