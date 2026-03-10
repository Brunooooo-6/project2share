using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WorldSwitchManager : MonoBehaviour
{
    public static WorldSwitchManager Instance;

    [Header("Scene Names")]
    public string world1Scene = "World_1";
    public string world2Scene = "World_2";
    public string world3Scene = "World_3";

    [Header("Player")]
    public Transform player;
    public CharacterController playerController;
    public LayerMask environmentMask = ~0;

    [Header("Input")]
    public KeyCode switchKey = KeyCode.P;

    [Header("Switch Safety")]
    public float overlapCheckRadius = 0.35f;
    public float bodyCheckHeight = 0.9f;
    public float headCheckHeight = 1.6f;
    public float forwardCheckDistance = 0.45f;
    public float groundCheckDistance = 1.2f;
    public bool blockSwitchIfUnsafe = true;

    [Header("Unsafe Position Fix")]
    public bool tryResolveUnsafePosition = true;
    public float resolveOffsetDistance = 0.4f;
    public float resolveUpOffset = 0.35f;

    [Header("Transition UI")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeOutDuration = 0.12f;
    public float fadeInDuration = 0.18f;

    [Header("Dizzy Effect")]
    public SimpleDizzyOverlay dizzyOverlay;
    public float dizzyDuration = 0.4f;

    [Header("Hint UI (Optional)")]
    public GameObject switchHintUI;          // 例如常驻显示“Press P to Switch”
    public TMP_Text statusText;              // 例如显示“Cannot switch here”
    public float statusTextDuration = 1.2f;

    private string currentWorld;
    private bool isSwitching = false;
    private Coroutine statusRoutine;

    public string CurrentWorld => currentWorld;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        if (switchHintUI != null)
            switchHintUI.SetActive(true);

        if (statusText != null)
            statusText.gameObject.SetActive(false);

        yield return LoadWorldOnly(world1Scene);
        currentWorld = world1Scene;
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            Debug.Log("[WorldSwitchManager] P pressed");
            TrySwitch();
        }
    }

    public void TrySwitch()
    {
        Debug.Log("[WorldSwitchManager] TrySwitch called");

        if (isSwitching)
        {
            Debug.Log("[WorldSwitchManager] Blocked: isSwitching = true");
            return;
        }

        string targetWorld = GetTargetWorldByRules();
        Debug.Log("[WorldSwitchManager] currentWorld = " + currentWorld);
        Debug.Log("[WorldSwitchManager] targetWorld = " + targetWorld);

        if (string.IsNullOrEmpty(targetWorld))
        {
            Debug.LogWarning("[WorldSwitchManager] Blocked: targetWorld is null or empty");
            return;
        }

        bool safe = CheckPlayerSpaceSafe();
        Debug.Log("[WorldSwitchManager] CheckPlayerSpaceSafe = " + safe);

        if (blockSwitchIfUnsafe && !safe)
        {
            Debug.LogWarning("[WorldSwitchManager] Blocked: unsafe position");
            ShowStatus("Cannot switch here");
            return;
        }

        Debug.Log("[WorldSwitchManager] Start switching to: " + targetWorld);
        StartCoroutine(SwitchRoutine(targetWorld));
    }

    private string GetTargetWorldByRules()
    {
        if (currentWorld == world1Scene)
            return world2Scene;

        if (currentWorld == world2Scene)
            return world3Scene;

        if (currentWorld == world3Scene)
            return world2Scene;

        return null;
    }

    private IEnumerator SwitchRoutine(string targetWorld)
    {
        isSwitching = true;

        if (playerController != null)
            playerController.enabled = false;

        // 淡出
        if (fadeCanvasGroup != null)
            yield return FadeTo(1f, fadeOutDuration);

        // 眩晕
        if (dizzyOverlay != null)
            dizzyOverlay.PlayDizzy(dizzyDuration);

        // 卸载当前世界
        if (!string.IsNullOrEmpty(currentWorld))
        {
            Scene oldScene = SceneManager.GetSceneByName(currentWorld);
            if (oldScene.IsValid() && oldScene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentWorld);
                while (!unloadOp.isDone)
                    yield return null;
            }
        }

        // 加载目标世界
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetWorld, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(targetWorld);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        yield return null;
        Physics.SyncTransforms();

        // 切换后再做一次安全检查
        if (!CheckPlayerSpaceSafe())
        {
            Debug.LogWarning("[WorldSwitchManager] Unsafe after switch.");

            if (tryResolveUnsafePosition)
            {
                TryResolvePosition();
                Physics.SyncTransforms();
            }

            if (!CheckPlayerSpaceSafe())
            {
                Debug.LogWarning("[WorldSwitchManager] Still unsafe after resolving.");
                ShowStatus("Bad overlap after switch");
            }
        }

        currentWorld = targetWorld;

        if (playerController != null)
            playerController.enabled = true;

        // 淡入
        if (fadeCanvasGroup != null)
            yield return FadeTo(0f, fadeInDuration);

        isSwitching = false;
    }

    private IEnumerator LoadWorldOnly(string targetWorld)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetWorld, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(targetWorld);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private bool CheckPlayerSpaceSafe()
    {
        if (player == null)
            return false;

        Vector3 feetPos = player.position + Vector3.up * 0.15f;
        Vector3 bodyPos = player.position + Vector3.up * bodyCheckHeight;
        Vector3 headPos = player.position + Vector3.up * headCheckHeight;

        // 1) 检查身体与墙/模型重叠
        bool feetOverlap = HasBlockingCollider(
            Physics.OverlapSphere(feetPos, overlapCheckRadius, environmentMask, QueryTriggerInteraction.Ignore)
        );

        bool bodyOverlap = HasBlockingCollider(
            Physics.OverlapSphere(bodyPos, overlapCheckRadius, environmentMask, QueryTriggerInteraction.Ignore)
        );

        bool headOverlap = HasBlockingCollider(
            Physics.OverlapSphere(headPos, overlapCheckRadius, environmentMask, QueryTriggerInteraction.Ignore)
        );

        // 2) 检查前方是不是紧贴墙
        bool frontBlocked = Physics.Raycast(
            player.position + Vector3.up * 1.0f,
            player.forward,
            forwardCheckDistance,
            environmentMask,
            QueryTriggerInteraction.Ignore
        );

        // 3) 检查脚下有没有地面
        bool hasGround = Physics.Raycast(
            player.position + Vector3.up * 0.2f,
            Vector3.down,
            groundCheckDistance,
            environmentMask,
            QueryTriggerInteraction.Ignore
        );

        bool safe = !feetOverlap && !bodyOverlap && !headOverlap && !frontBlocked && hasGround;
        return safe;
    }

    private bool HasBlockingCollider(Collider[] hits)
    {
        foreach (Collider col in hits)
        {
            if (col == null) continue;

            if (playerController != null && col == playerController)
                continue;

            if (col.transform == player)
                continue;

            if (col.transform.IsChildOf(player))
                continue;

            if (col.isTrigger)
                continue;

            return true;
        }

        return false;
    }

    private void TryResolvePosition()
    {
        if (player == null) return;

        Vector3 original = player.position;

        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,
            Vector3.up * resolveUpOffset,
            player.forward * resolveOffsetDistance,
            -player.forward * resolveOffsetDistance,
            player.right * resolveOffsetDistance,
            -player.right * resolveOffsetDistance,
            (Vector3.up * resolveUpOffset) + player.forward * resolveOffsetDistance,
            (Vector3.up * resolveUpOffset) - player.forward * resolveOffsetDistance,
            (Vector3.up * resolveUpOffset) + player.right * resolveOffsetDistance,
            (Vector3.up * resolveUpOffset) - player.right * resolveOffsetDistance
        };

        foreach (Vector3 offset in offsets)
        {
            player.position = original + offset;
            Physics.SyncTransforms();

            if (CheckPlayerSpaceSafe())
            {
                Debug.Log("[WorldSwitchManager] Position resolved: " + offset);
                return;
            }
        }

        player.position = original;
    }

    private void ShowStatus(string message)
    {
        if (statusText == null) return;

        if (statusRoutine != null)
            StopCoroutine(statusRoutine);

        statusRoutine = StartCoroutine(ShowStatusRoutine(message));
    }

    private IEnumerator ShowStatusRoutine(string message)
    {
        statusText.gameObject.SetActive(true);
        statusText.text = message;

        yield return new WaitForSeconds(statusTextDuration);

        statusText.gameObject.SetActive(false);
        statusRoutine = null;
    }
}