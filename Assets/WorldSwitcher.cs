using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSwitcher : MonoBehaviour
{
    [Header("Scene Names (must match exactly)")]
    [SerializeField] private string worldA = "World_A";
    [SerializeField] private string worldB = "World_B";

    [Header("Player Transform (drag your FPC here)")]
    [SerializeField] private Transform player;

    [Header("Transition FX")]
    [SerializeField] private float lockDuration = 0.15f;   // 短暂停顿像“眩晕”
    [SerializeField] private float flickerDuration = 0.10f; // 可配合后续做UI闪烁

    private bool inWorldA = true;
    private bool isSwitching = false;

    private void Start()
    {
        // 确保A是主场景，B未加载
        // 你也可以改成：开局把B预加载但禁用，这样切换更快
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TrySwitch();
        }
    }

    private void TrySwitch()
    {
        if (isSwitching) return;
        if (player == null)
        {
            Debug.LogError("WorldSwitcher: Player Transform not assigned.");
            return;
        }

        StartCoroutine(SwitchRoutine());
    }

    private IEnumerator SwitchRoutine()
    {
        isSwitching = true;

        // 记录玩家位置/朝向
        Vector3 pos = player.position;
        Quaternion rot = player.rotation;

        // 可选：临时冻结控制（最简单做法：禁用CharacterController）
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 小停顿（营造“眩晕”）
        yield return new WaitForSeconds(lockDuration);

        if (inWorldA)
        {
            // 加载B（Additive）并设为活跃场景
            yield return SceneManager.LoadSceneAsync(worldB, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(worldB));

            // 卸载A的环境？——不建议卸载A，因为玩家对象在A里
            // 所以我们保留A，但你可以把A的环境放到单独场景里再卸载
        }
        else
        {
            // 卸载B，回到A
            yield return SceneManager.UnloadSceneAsync(worldB);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(worldA));
        }

        // 还原玩家位置/朝向（保持不变）
        player.position = pos;
        player.rotation = rot;

        // 再等一点点（你之后可用UI或后处理做闪烁）
        yield return new WaitForSeconds(flickerDuration);

        if (cc != null) cc.enabled = true;

        inWorldA = !inWorldA;
        isSwitching = false;
    }
}