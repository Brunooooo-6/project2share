
using System.Collections;
using UnityEngine;

public class EnterSoundTrigger : MonoBehaviour 
{
    public AudioSource audioSource; 
    public float audioPlayDuration = 10f; 
    public float fadeOutDuration = 2f;
    public bool allowTriggerAgain = false; 

    private Coroutine fadeOutCoroutine;
    private bool hasBeenTriggered = false; 

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("⚠️ AudioSource is not assigned to " + gameObject.name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("🔵 Collision Detected with: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("✅ Player entered trigger area.");

            if (audioSource == null)
            {
                Debug.LogError("⚠️ AudioSource is missing!");
                return;
            }

            if (!audioSource.isPlaying && (!hasBeenTriggered || allowTriggerAgain))
            {
                Debug.Log("🎵 Playing sound: " + audioSource.clip.name);
                audioSource.Play();
                hasBeenTriggered = true; 

                if (fadeOutCoroutine != null)
                {
                    StopCoroutine(fadeOutCoroutine);
                    audioSource.volume = 1f;
                }

                StartCoroutine(StopAudioAfterTime(audioPlayDuration));
            }
            else
            {
                Debug.Log("⚠️ Sound already playing or trigger blocked.");
            }
        }
    }

    IEnumerator StopAudioAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        fadeOutCoroutine = StartCoroutine(FadeOutAudio(fadeOutDuration));
    }

    IEnumerator FadeOutAudio(float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;

        if (allowTriggerAgain)
        {
            hasBeenTriggered = false;
        }

        Debug.Log("🔇 Sound faded out.");
    }
}

