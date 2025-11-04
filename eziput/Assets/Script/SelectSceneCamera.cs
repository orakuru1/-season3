using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectSceneCamera : MonoBehaviour
{
    [Header("カメラ設定")]
    public Camera mainCamera;
    public float rotateSpeed = 10f;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1.5f;

    [Header("フェード設定")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.2f;

    private Vector3 initialPosition;
    private bool isTransitioning = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        initialPosition = mainCamera.transform.position;

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1;
            StartCoroutine(FadeIn());
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // カメラをゆるく回転させる（演出用）
        mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, rotateSpeed * Time.deltaTime);

        // 上下にゆっくり浮遊させる
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        mainCamera.transform.position = initialPosition + new Vector3(0, yOffset, 0);
    }

    // フェードイン演出
    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = 1 - (t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0;
    }

    // 他シーンへ遷移（ボタンなどから呼び出し）
    public void TransitionToScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isTransitioning = true;
        if (fadeCanvas != null)
        {
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvas.alpha = t / fadeDuration;
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);
    }
}
