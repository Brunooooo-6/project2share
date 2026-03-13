using UnityEngine;

public class AimToOutlineLayer : MonoBehaviour
{
    public Camera cam;
    public float range = 6f;
    public LayerMask aimMask = ~0;          // 你想被射线命中的层（包含 Crate）
    public string outlineLayerName = "Outline";

    private GameObject last;
    private int lastLayer;

    void Update()
    {
        if (!cam) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, aimMask, QueryTriggerInteraction.Ignore))
        {
            GameObject target =
                hit.collider.attachedRigidbody != null ? hit.collider.attachedRigidbody.gameObject :
                hit.collider.gameObject;

            if (target != last)
            {
                ClearLast();
                SetOutlined(target);
            }
        }
        else
        {
            ClearLast();
        }
    }

    private void SetOutlined(GameObject go)
    {
        int outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        if (outlineLayer < 0)
        {
            Debug.LogError($"Layer '{outlineLayerName}' not found.");
            return;
        }

        last = go;
        lastLayer = go.layer;
        go.layer = outlineLayer;
    }

    private void ClearLast()
    {
        if (last == null) return;

        last.layer = lastLayer;
        last = null;
    }
}