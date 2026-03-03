using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("UI")]
    public Canvas transitionCanvas;
    public Image fadeImage;              // 全屏黑色 Image
    public RectTransform dizzyOverlay;   // 可选：旋转/缩放的叠加图层

    [Header("Timing")]
    public float fadeInTime = 0.25f;
    public float holdTime = 0.05f;
    public float fadeOutTime = 0.25f;
    public float dizzySpinSpeed = 180f;  // 每秒旋转度数

    [Header("Player")]
    public string playerTag = "Player";

    bool isTransitioning;

    // 要保存的信息
    Vector3 savedPos;
    Quaternion savedRot;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
        if (transitionCanvas != null) DontDestroyOnLoad(transitionCanvas.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // 眩晕：让 overlay 一直转（在转场期间更明显）
        if (isTransitioning && dizzyOverlay != null)
        {
            dizzyOverlay.Rotate(0, 0, dizzySpinSpeed * Time.unscaledDeltaTime);
        }
    }

    public void TransitionTo(string targetScene)
    {
        if (isTransitioning) return;
        StartCoroutine(DoTransition(targetScene));
    }

    IEnumerator DoTransition(string targetScene)
    {
        isTransitioning = true;
        Time.timeScale = 1f; // 确保不是暂停状态（眩晕用 unscaled 也行）

        // 找玩家并保存位置
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            savedPos = player.transform.position;
            savedRot = player.transform.rotation;
        }

        // UI 打开
        if (transitionCanvas != null) transitionCanvas.enabled = true;

        // 眩晕淡入（遮住加载）
        yield return Fade(0f, 1f, fadeInTime);

        // 让眩晕在纯黑里停一小下，避免闪
        yield return new WaitForSecondsRealtime(holdTime);

        // 异步加载
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        // 等一帧，让新场景对象都起来
        yield return null;

        // 在新场景里恢复玩家位置/朝向
        RestorePlayerTransform();

        // 眩晕淡出
        yield return Fade(1f, 0f, fadeOutTime);

        // 关闭 UI（可选）
        if (transitionCanvas != null) transitionCanvas.enabled = false;

        isTransitioning = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 这里也可以做一些按场景不同处理（比如找锚点）
    }

    void RestorePlayerTransform()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        // 如果你的角色是 CharacterController，直接改 transform 会有问题，需要先禁用
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(savedPos, savedRot);

        if (cc != null) cc.enabled = true;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null || duration <= 0f)
        {
            if (fadeImage != null)
            {
                var c0 = fadeImage.color;
                c0.a = to;
                fadeImage.color = c0;
            }
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            var c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        var cEnd = fadeImage.color;
        cEnd.a = to;
        fadeImage.color = cEnd;
    }
}