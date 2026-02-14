using UnityEngine;

public class VerticalLadder : MonoBehaviour
{
    [SerializeField] private float exitSideForce = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out var movement))
        {
            if (movement.isClimbing) return;

            Vector2 input = movement.moveAction.action.ReadValue<Vector2>();

            if (input.y > 0.1f)
            {
                movement.EnterLadder(this.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.ExitLadder();
        }
    }
}