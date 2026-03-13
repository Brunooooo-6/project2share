using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleDizzyOverlay : MonoBehaviour
{
    public RectTransform dizzyRect;
    public Image dizzyImage;

    [Header("Animation")]
    public float maxAlpha = 0.35f;
    public float maxRotation = 12f;
    public float maxScale = 1.08f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        ResetVisual();
    }

    public void PlayDizzy(float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(DizzyRoutine(duration));
    }

    private IEnumerator DizzyRoutine(float duration)
    {
        if (dizzyRect == null || dizzyImage == null)
            yield break;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            // 先强后弱
            float wave = Mathf.Sin(p * Mathf.PI);
            float rot = Mathf.Sin(t * 18f) * maxRotation * wave;
            float scale = Mathf.Lerp(1f, maxScale, wave);
            float alpha = wave * maxAlpha;

            dizzyRect.localRotation = Quaternion.Euler(0f, 0f, rot);
            dizzyRect.localScale = new Vector3(scale, scale, 1f);

            Color c = dizzyImage.color;
            c.a = alpha;
            dizzyImage.color = c;

            yield return null;
        }

        ResetVisual();
        currentRoutine = null;
    }

    private void ResetVisual()
    {
        if (dizzyRect != null)
        {
            dizzyRect.localRotation = Quaternion.identity;
            dizzyRect.localScale = Vector3.one;
        }

        if (dizzyImage != null)
        {
            Color c = dizzyImage.color;
            c.a = 0f;
            dizzyImage.color = c;
        }
    }
}