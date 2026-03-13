using UnityEngine;

public class PushRigidbodies : MonoBehaviour
{
    public float pushPower = 2.5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        rb.AddForce(pushDir * pushPower, ForceMode.Impulse);
    }
}