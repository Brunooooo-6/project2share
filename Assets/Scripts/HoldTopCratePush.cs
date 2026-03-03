using UnityEngine;
using UnityEngine.InputSystem;

public class HoldRPushTopCrate : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask crateMask = ~0;

    [Header("Push Settings")]
    public float range = 3.5f;
    public float pushForce = 40f;       // 持续力（每秒），比 impulse 温和
    public float maxForce = 80f;        // 防止过猛
    public float upBias = 0f;           // 一般保持 0
    public float hitOffset = 0.02f;     // 避免力点过于贴面

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // 按住 R 才推
        if (!kb.rKey.isPressed) return;

        TryHoldPushTopCrate();
    }

    void TryHoldPushTopCrate()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, range, crateMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return;

        Rigidbody bestRb = null;
        float bestY = float.NegativeInfinity;
        Vector3 bestPoint = Vector3.zero;

        foreach (var h in hits)
        {
            Rigidbody rb = h.collider.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            float y = rb.worldCenterOfMass.y;   // 选最高箱子
            if (y > bestY)
            {
                bestY = y;
                bestRb = rb;
                bestPoint = h.point;
            }
        }

        if (bestRb == null) return;

        // 水平推（避免往上/往下顶）
        Vector3 dir = playerCamera.transform.forward;
        dir.y = upBias;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        // 持续力：与 dt 相关，用 Acceleration 更稳定（与质量无关），或用 Force（与质量有关）
        float f = Mathf.Min(maxForce, pushForce);

        // 在命中点稍微偏离一点，避免数值抖动
        Vector3 point = bestPoint + dir * hitOffset;

        bestRb.AddForceAtPosition(dir * f, point, ForceMode.Acceleration);
    }
}