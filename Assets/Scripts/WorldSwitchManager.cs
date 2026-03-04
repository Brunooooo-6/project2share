using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldStateManager : MonoBehaviour
{
    [Header("World Roots")]
    public GameObject worldNormal;
    public GameObject worldMutated;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.P;

    [Header("Transition UI")]
    public Canvas transitionCanvas;
    public Image fadeImage;                 // 全屏黑
    public RectTransform dizzyOverlay;      // 可选：旋转叠加

    [Header("Timing (seconds)")]
    public float fadeInTime = 0.22f;
    public float holdTime = 0.06f;          // 黑屏中间停一下更稳
    public float fadeOutTime = 0.22f;

    [Header("Dizzy")]
    public float dizzySpinSpeed = 260f;     // 度/秒

    bool isMutated = false;
    bool isBusy = false;

    void Start()
    {
        // 初始化世界状态：Normal 开，Mutated 关
        if (worldNormal != null) worldNormal.SetActive(true);
        if (worldMutated != null) worldMutated.SetActive(false);

        if (transitionCanvas != null) transitionCanvas.enabled = false;
        SetFadeAlpha(0f);
    }

    void Update()
    {
        if (isBusy && dizzyOverlay != null)
        {
            dizzyOverlay.Rotate(0, 0, dizzySpinSpeed * Time.unscaledDeltaTime);
        }

        if (!isBusy && Input.GetKeyDown(toggleKey))
        {
            StartCoroutine(ToggleWorld());
        }
    }

    IEnumerator ToggleWorld()
    {
        isBusy = true;

        // 打开 UI
        if (transitionCanvas != null) transitionCanvas.enabled = true;

        // 淡入到黑（遮住切换）
        yield return Fade(0f, 1f, fadeInTime);
        yield return new WaitForSecondsRealtime(holdTime);

        // 在黑屏时切换世界（无缝关键）
        isMutated = !isMutated;
        if (worldNormal != null) worldNormal.SetActive(!isMutated);
        if (worldMutated != null) worldMutated.SetActive(isMutated);

        // 淡出
        yield return Fade(1f, 0f, fadeOutTime);

        // 关闭 UI
        if (transitionCanvas != null) transitionCanvas.enabled = false;

        isBusy = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null || duration <= 0f)
        {
            SetFadeAlpha(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetFadeAlpha(a);
            yield return null;
        }
        SetFadeAlpha(to);
    }

    void SetFadeAlpha(float a)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}