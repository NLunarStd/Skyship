using UnityEngine;

public class SteeringController : MonoBehaviour
{
    public bool ShipControlMode = false;
    public ShipController ship;

    public float rudderChangeSpeed = 60f;  // หมุนเร็วแค่ไหน
    public float returnSpeed = 30f;        // คืนกลางเร็วแค่ไหน

    private void OnEnable()
    {
        EventManager.OnEnterShipControl += EnterShipControlMode;
        EventManager.OnExitShipControl += ExitShipControlMode;
    }

    private void OnDisable()
    {
        EventManager.OnEnterShipControl -= EnterShipControlMode;
        EventManager.OnExitShipControl -= ExitShipControlMode;  
    }

    void Update()
    {
        if (ShipControlMode)
        {
            float input = 0f;

            if (Input.GetKey(KeyCode.A)) input = -1f;
            if (Input.GetKey(KeyCode.D)) input = 1f;

            if (input != 0)
            {
                ship.rudderAngle += input * rudderChangeSpeed * Time.deltaTime;
            }
            else
            {
                // คืนสู่ตรงกลาง
                ship.rudderAngle = Mathf.MoveTowards(
                    ship.rudderAngle,
                    0f,
                    returnSpeed * Time.deltaTime
                );
            }

            ship.rudderAngle = Mathf.Clamp(ship.rudderAngle, -ship.maxRudder, ship.maxRudder);
        }
    }

    void EnterShipControlMode()
    {
        ShipControlMode = true;
        Debug.Log("ENTERED SHIP CONTROL MODE");
    }

    void ExitShipControlMode()
    {
        ShipControlMode = false;
        Debug.Log("EXITED SHIP CONTROL MODE");
    }
}
