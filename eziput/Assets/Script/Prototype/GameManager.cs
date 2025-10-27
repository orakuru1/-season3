using UnityEngine;
using UnityEngine.SceneManagement;

public enum RouteType { Safe, Danger }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentStage = 1;
    public RouteType currentRoute = RouteType.Safe;

    [Header("Environment References")]
    [SerializeField] private Light mainLight;
    [SerializeField] private Material safeFloorMaterial;
    [SerializeField] private Material dangerFloorMaterial;
    [SerializeField] private Renderer[] floorRenderers;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyEnvironmentSettings();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンロード後に環境設定を再適用
        ApplyEnvironmentSettings();
    }

    private void ApplyEnvironmentSettings()
    {
        // メインライトが未設定なら探す
        if (mainLight == null)
            mainLight = FindObjectOfType<Light>();

        // 床Rendererを再取得
        floorRenderers = FindObjectsOfType<Renderer>();

        if (currentRoute == RouteType.Safe)
        {
            // Safeルート設定
            if (mainLight != null)
            {
                mainLight.color = Color.white;
                mainLight.intensity = 1.2f;
            }

            RenderSettings.ambientLight = new Color(0.7f, 0.8f, 1.0f);

            foreach (var r in floorRenderers)
            {
                if (r.CompareTag("Floor"))
                    r.material = safeFloorMaterial;
            }
        }
        else
        {
            // Dangerルート設定
            if (mainLight != null)
            {
                mainLight.color = new Color(1f, 0.5f, 0.3f);
                mainLight.intensity = 0.8f;
            }

            RenderSettings.ambientLight = new Color(0.3f, 0.1f, 0.1f);

            foreach (var r in floorRenderers)
            {
                if (r.CompareTag("Floor"))
                    r.material = dangerFloorMaterial;
            }
        }
    }

    public void SetRoute(RouteType route)
    {
        currentRoute = route;
    }
}
