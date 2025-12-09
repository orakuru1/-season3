using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float duration = 1.5f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }
    }
}
