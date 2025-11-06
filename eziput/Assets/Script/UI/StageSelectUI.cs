using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private Button safeButton;
    [SerializeField] private Button dangerButton;

    private void Start()
    {
        if (safeButton != null)
            safeButton.onClick.AddListener(() => GameManager.Instance.SelectSafeRoute());
        if (dangerButton != null)
            dangerButton.onClick.AddListener(() => GameManager.Instance.SelectDangerRoute());
    }
}
