using UnityEngine;

public class ShipController : MonoBehaviour
{
    public  bool EngineOn;

    public float moveSpeed = 5f;

    [Header("Rudder")]
    public float rudderAngle = 0f;        // มุมปัจจุบัน
    public float maxRudder = 45f;        // เอียงได้สุด
    public float turnSpeed = 20f;        // เรือหมุนแรงแค่ไหน

    void Update()
    {
        if (EngineOn)
        {
            SailShip();
        }
    }

    void SailShip()
    {
        // เดินหน้าเสมอ
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // หมุนตามมุมหางเสือ
        float turn = rudderAngle / maxRudder; // แปลงเป็น 0 ? 1
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
    }

}
