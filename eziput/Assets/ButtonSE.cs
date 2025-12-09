using UnityEngine;
using UnityEngine.UI;

public class ButtonSE : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip seClip;

    public void PlaySE()
    {
        if (audioSource != null && seClip != null)
        {
            audioSource.PlayOneShot(seClip);
        }
    }
}
