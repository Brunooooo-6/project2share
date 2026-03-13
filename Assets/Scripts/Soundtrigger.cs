using UnityEngine;

public class TriggerEnterSFX : MonoBehaviour
{
    public AudioSource audioSource; // Assign an Audio Source in Inspector

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Only trigger when Player enters
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
