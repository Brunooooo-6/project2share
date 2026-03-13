using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Pickup")]
    public float pickupDistance = 3f;
    public LayerMask pickupMask = ~0;

    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("Drop")]
    public float dropForwardOffset = 1.0f;
    public float dropUpOffset = 0.2f;

    private CarryPickup carriedItem;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (carriedItem == null)
            {
                TryPickup();
            }
            else
            {
                DropCurrent();
            }
        }
    }

    private void TryPickup()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[PlayerCarryController] playerCamera is missing.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupMask, QueryTriggerInteraction.Ignore))
        {
            CarryPickup pickup = hit.collider.GetComponentInParent<CarryPickup>();

            if (pickup != null && !pickup.isCarried)
            {
                carriedItem = pickup;
                carriedItem.PickUp(holdPoint);

                Debug.Log("[PlayerCarryController] Picked up: " + pickup.name);
            }
        }
    }

    private void DropCurrent()
    {
        if (carriedItem == null) return;

        CarryPickup item = carriedItem;
        carriedItem = null;

        item.Drop();

        // 放到玩家前面一点的位置
        item.transform.position =
            playerCamera.transform.position +
            playerCamera.transform.forward * dropForwardOffset +
            Vector3.up * dropUpOffset;

        Debug.Log("[PlayerCarryController] Dropped: " + item.name);
    }

    public bool IsCarrying()
    {
        return carriedItem != null;
    }

    public CarryPickup GetCarriedItem()
    {
        return carriedItem;
    }
}