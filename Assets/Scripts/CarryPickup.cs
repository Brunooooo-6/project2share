using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarryPickup : MonoBehaviour
{
    [Header("Info")]
    public string itemID = "CarryItem_A";

    [Header("Optional State Trigger On Pickup")]
    public bool triggerStateOnPickup = false;
    public string pickupStateKey = "Fountain_Offering_Done";

    [HideInInspector] public bool isCarried = false;

    private Rigidbody rb;
    private Collider[] allColliders;
    private bool hasTriggeredPickupState = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>();
    }

    public void PickUp(Transform holdPoint)
    {
        isCarried = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        foreach (Collider col in allColliders)
        {
            col.enabled = false;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (triggerStateOnPickup && !hasTriggeredPickupState)
        {
            if (PuzzleStateManager.Instance != null)
            {
                PuzzleStateManager.Instance.SetState(pickupStateKey, true);
                Debug.Log("[CarryPickup] Pickup triggered state: " + pickupStateKey);
            }

            hasTriggeredPickupState = true;
        }
    }

    public void Drop()
    {
        isCarried = false;

        transform.SetParent(null);
        rb.isKinematic = false;

        foreach (Collider col in allColliders)
        {
            col.enabled = true;
        }
    }
}