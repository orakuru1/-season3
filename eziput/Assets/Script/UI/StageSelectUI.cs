using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StageSelectUI : MonoBehaviour
{
    public Button safeButton;
    public Button dangerButton;
    public TextMeshProUGUI titleText;

    private bool selected = false;

    void Start()
    {
        titleText.text = "Select Route";

        safeButton.onClick.AddListener(() => SelectRoute(RouteType.Safe));
        dangerButton.onClick.AddListener(() => SelectRoute(RouteType.Danger));
    }

    void SelectRoute(RouteType route)
    {
        if (selected) return; // すでに選択済みなら無視
        selected = true;

        GameManager.Instance.SetRoute(route);
        Debug.Log($"Route selected: {route}");

        // ボタンを無効化して見た目も変える
        safeButton.interactable = false;
        dangerButton.interactable = false;
        titleText.text = $"Selected: {route} Route";

        // 少し待って次のシーンへ
        Invoke(nameof(LoadNextScene), 1.5f);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("DungeonScene");
    }
}
