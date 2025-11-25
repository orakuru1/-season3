using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 現在選択されているルート
    public RouteType CurrentRoute { get; private set; } = RouteType.Safe;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectSafeRoute()
    {
        CurrentRoute = RouteType.Safe;
        Debug.Log("Safeルートを選択しました。");
        SceneManager.LoadScene("Tougou");
    }

    public void SelectDangerRoute()
    {
        CurrentRoute = RouteType.Danger;
        Debug.Log("Dangerルートを選択しました。");
        SceneManager.LoadScene("Tougou");
    }

    public void LoadSelectScene()
    {
        SceneManager.LoadScene("Scene_Select");
    }
}
