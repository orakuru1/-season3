using UnityEngine;

public enum RouteType { None, Safe, Danger }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int stage = 1;
    public RouteType route = RouteType.None;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("GameManager Initialized.");
    }

    public void SetRoute(RouteType newRoute)
    {
        route = newRoute;
        Debug.Log($"✅ Route set: {route}");
        LogStatus();
    }

    public void LogStatus()
    {
        Debug.Log($"GameManager: stage={stage}, route={route}");
    }
}
