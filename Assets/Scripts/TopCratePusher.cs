using UnityEngine;
using UnityEngine.InputSystem;

public class TopCratePusher : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;                 // 拖 Main Camera
    public LayerMask crateMask;                 // 只选 Crate Layer

    [Header("Push Tuning")]
    public float range = 3.0f;                  // 推的距离
    public float pushImpulse = 8f;              // 推力（冲量）
    public float upBias = 0.0f;                 // 需要的话给一点点上抬（建议0）
    public float cooldown = 0.2f;               // 推的冷却

    private float nextTime;

    void Update()
    {
        // 每帧验证脚本确实在跑（你已经看到了）
        // Debug.Log("TopCratePusher Update running");

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null)
        {
            Debug.LogWarning("TopCratePusher: Keyboard.current is NULL");
            return;
        }

        if (kb.eKey.isPressed) // 用 isPressed（持续按）比 wasPressedThisFrame 更不容易错过
        {
            Debug.LogWarning("TopCratePusher: E IS PRESSED");
            TryPushTopCrate();
        }
    }

    void TryPushTopCrate()
    {
        Debug.LogWarning("TryPushTopCrate called");

        if (playerCamera == null)
        {
            Debug.LogError("playerCamera is NULL");
            return;
        }

        // 画射线方便你在 Scene 视图看方向
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * range, Color.yellow, 1f);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // 先无视 Layer，直接 Everything（保证命中）
        RaycastHit[] hits = Physics.RaycastAll(ray, range, ~0, QueryTriggerInteraction.Ignore);
        Debug.LogWarning($"Raycast hits: {hits.Length} (range={range})");

        if (hits.Length == 0) return;

        Rigidbody bestRb = null;
        float bestY = float.NegativeInfinity;
        Vector3 bestPoint = Vector3.zero;

        foreach (var h in hits)
        {
            Debug.LogWarning($"Hit: {h.collider.name}, layer={h.collider.gameObject.layer}, hasRB={(h.collider.attachedRigidbody != null)}");

            var rb = h.collider.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            float y = rb.worldCenterOfMass.y;
            if (y > bestY)
            {
                bestY = y;
                bestRb = rb;
                bestPoint = h.point;
            }
        }

        if (bestRb == null)
        {
            Debug.LogWarning("No rigidbody found in hits.");
            return;
        }

        // 水平推
        Vector3 dir = playerCamera.transform.forward;
        dir.y = 0f;
        dir.Normalize();

        float impulse = 20f; // 测试用，先大一点确认效果
        bestRb.AddForceAtPosition(dir * impulse, bestPoint, ForceMode.Impulse);

        Debug.LogWarning($"PUSHED: {bestRb.name} impulse={impulse}");
    }
}