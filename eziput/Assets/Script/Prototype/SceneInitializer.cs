using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private Light mainLight;

    private void Start()
    {
        // Light自動検出
        if (mainLight == null)
        {
            mainLight = FindObjectOfType<Light>();
            if (mainLight == null)
            {
                Debug.LogWarning("Directional Light が見つかりません。");
                return;
            }
        }

        ApplyLighting();
    }

    private void ApplyLighting()
    {
        var route = GameManager.Instance.CurrentRoute;
        Debug.Log($"照明設定を適用: {route}");

        if (route == RouteType.Safe)
        {
            mainLight.color = new Color(0.8f, 0.9f, 1.0f);
            mainLight.intensity = 1.2f;
        }
        else
        {
            mainLight.color = new Color(1.0f, 0.5f, 0.5f);
            mainLight.intensity = 0.8f;
        }
    }
}
