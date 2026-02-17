using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItemHandler : MonoBehaviour
{
    [Header("Hold Settings")]
    [SerializeField] private Transform holdAnchor;
    [SerializeField] private float liftDistance = 2.5f;
    [SerializeField] private Vector3 liftRayOffset;
    [SerializeField] private Vector3 characterHoldOffset = new Vector3(0, -1f, 0);
    public bool IsHoldingItem => currentlyHeldObject != null;

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float chargeTimeForMaxPower = 2f; 

    [Header("Throw Physics Tuning")]
    [SerializeField] private float upwardArcForce = 0.25f; // How much it lifts (0 to 1)
    [SerializeField] private float tumbleIntensity = 5f;




    private GameObject currentlyHeldObject;
    private IPickable currentPickable;
    private float pickPressedTime;
    private bool isChargingThrow;


    [Header("Input References")]
    public InputActionReference pickAction;

    private void OnEnable()
    {
        EventManager.Subscribe<CharacterStunnedEvent>(OnPlayerStunned);
    }

    private void OnDisable()
    {
        EventManager.UnSubscribe<CharacterStunnedEvent>(OnPlayerStunned);
    }


    private void Update()
    {
        HandlePickInput();
    }
    public void HandlePickInput()
    {
        OnPressedButton();
        OnReleaseButton();

    }

    private void OnPressedButton()
    {
        if (pickAction.action.WasPressedThisFrame())
        {
            if (currentlyHeldObject == null)
            {
                TryPickUp();
            }
            else
            {
                pickPressedTime = Time.time;
                isChargingThrow = true;
            }
        }
    }

    private void OnReleaseButton()
    {
        if (pickAction.action.WasReleasedThisFrame() && isChargingThrow)
        {
            float holdDuration = Time.time - pickPressedTime;
            isChargingThrow = false;

            if (holdDuration < 0.5f)
            {
                DropItem();
            }
            else
            {
                // Calculate power based on time held (clamped between 1s and chargeTime)
                float chargeRatio = Mathf.InverseLerp(1.0f, chargeTimeForMaxPower, holdDuration);
                float finalForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);
                ThrowItem(finalForce);
            }
        }
    }

    private void TryPickUp()
    {
        Ray ray = new Ray(transform.position + liftRayOffset, transform.forward);
        //Debug.DrawRay(ray.origin, ray.direction * liftDistance, Color.red, 1f);
        //Debug.Log("Attempting to pick up item...");

        if (Physics.Raycast(ray, out RaycastHit hit, liftDistance))
        {
            var pickable = hit.collider.GetComponent<IPickable>();

            // Condition: 
            // 1. It must be pickable.
            // 2. If it's an enemy (no PlayerMovement), it MUST be stunned.
            // 3. If it's a player, they can be picked anytime.

            if (pickable == null) return; // Exit early if not pickable at all

            bool isEnemy = hit.collider.GetComponent<EnemyHealth>() != null;
            bool isPlayer = hit.collider.GetComponent<PlayerMovement_old>() != null;
            var targetStun = hit.collider.GetComponent<StunHandler>();

            bool canPickEnemy = isEnemy && (targetStun != null && targetStun.IsStunned);
            bool canPickOther = !isEnemy; // Covers both Players and Items

            if ((canPickEnemy || canPickOther) && hit.collider.transform.parent == null)
            {
                currentlyHeldObject = hit.collider.gameObject;

                Physics.IgnoreCollision(GetComponent<Collider>(), hit.collider, true);

                currentPickable = pickable;

                currentPickable.PickUp();

                currentlyHeldObject.transform.SetParent(holdAnchor);

                bool isCharacter = currentlyHeldObject.GetComponent<StunHandler>() != null;
                currentlyHeldObject.transform.localPosition = isCharacter ? characterHoldOffset : Vector3.zero;

                currentlyHeldObject.transform.localRotation = Quaternion.identity;

                if (currentlyHeldObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {

                    rb.linearVelocity = Vector3.zero; 
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            }
        }
    }

    private void ThrowItem(float force)
    {
        GameObject item = currentlyHeldObject;
        var throwable = item.GetComponent<IThrowable>();

        ReleasePhysics(item);

        if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 throwDir = (transform.forward + (transform.up * upwardArcForce)).normalized;

            rb.AddForce(throwDir * force, ForceMode.Impulse);

            Vector3 torqueDirection = transform.right;
            rb.AddTorque(torqueDirection * tumbleIntensity, ForceMode.Impulse);
        }

        throwable?.Throw(transform.forward, force);
        ClearReferences();
    }

    private void DropItem()
    {
        currentPickable?.Drop();
        ReleasePhysics(currentlyHeldObject);
        ClearReferences();
    }
    private void OnPlayerStunned(CharacterStunnedEvent e)
    {
        if (e.Victim == this.gameObject)
        {
            DropItem();
        }
    }
    private void ReleasePhysics(GameObject item)
    {
        if (item == null) return;
        if (item.TryGetComponent<Collider>(out Collider col))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
        }

        item.transform.SetParent(null);
        if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }
    }

    private void ClearReferences()
    {
        currentlyHeldObject = null;
        currentPickable = null;
        isChargingThrow = false;
    }


}