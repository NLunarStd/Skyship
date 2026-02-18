using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringController : MonoBehaviour
{
    public bool ShipControlMode = false;
    public ShipController ship;

    public float rudderChangeSpeed = 60f;  // หมุนเร็วแค่ไหน
    public float returnSpeed = 30f;        // คืนกลางเร็วแค่ไหน

    [Header("Input Action Reference")]
    public InputActionReference turnLeft;
    public InputActionReference turnRight;
    private void OnEnable()
    {
        /*
        EventManager2.OnEnterShipControl += EnterShipControlMode;
        EventManager2.OnExitShipControl += ExitShipControlMode;
        */
        EventManager.Subscribe<CharacterControlRudderEvent>(OnCharacterControlRudder);
    }

    private void OnDisable()
    {
        /*
        EventManager2.OnEnterShipControl -= EnterShipControlMode;
        EventManager2.OnExitShipControl -= ExitShipControlMode;
        */
        EventManager.UnSubscribe<CharacterControlRudderEvent>(OnCharacterControlRudder);
    }

    void Update()
    {
        if (ShipControlMode)
        {
            float input = 0f;

            if (turnLeft.action.IsPressed()) input = -1f;
            if (turnRight.action.IsPressed()) input = 1f;

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
        else if (!ShipControlMode)
        {
            if(ship.rudderAngle != 0f)
            {
                ship.rudderAngle = Mathf.MoveTowards(
                    ship.rudderAngle,
                    0f,
                    returnSpeed * Time.deltaTime
                );
            }
        }

    }
    /*
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
    */

    void OnCharacterControlRudder(CharacterControlRudderEvent e)
    {
        ShipControlMode = e.value;
        Debug.Log("ShipControlMode: " + e.value);
    }

}
