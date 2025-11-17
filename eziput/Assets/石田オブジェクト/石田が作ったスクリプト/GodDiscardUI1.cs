using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodDiscardUI1 : MonoBehaviour
{
    public static GodDiscardUI1 Instance{ get; private set; }
    [SerializeField] private Button ketteiButton;
    [SerializeField] private Button GodSkillButtonPrefab;
    [SerializeField] private Transform GodSkillDisPanel;

    private int PushID = -1;

    private System.Action<int> onNumberAction;//選んだスキル番号

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        ketteiButton.onClick.AddListener(() => OnClickketteiButton());
        AllHide();
    }

    public void Show(List<GodData> gods, System.Action<int> action)
    {
        PushID = -1;
        foreach (var god in gods)
        {
            Button button = Instantiate(GodSkillButtonPrefab, GodSkillDisPanel);
            button.onClick.AddListener(() => OnclickGodButton(god.id));
            //見た目の処理↓
            Image image = button.GetComponent<Image>();
            image.sprite = god.icon;
        }
        onNumberAction = action;
        AllShow();
    }

    public void OnclickGodButton(int id)
    {
        PushID = id;
    }
    
    public void OnClickketteiButton()
    {
        if (PushID == -1) return;
        foreach (Transform t in GodSkillDisPanel)
            Destroy(t.gameObject);
        onNumberAction?.Invoke(PushID);//神の力を消す処理、GodPlayerから呼ぶのが良いか。
        AllHide();
    }

    public void AllHide()
    {
        ketteiButton.gameObject.SetActive(false);
        GodSkillDisPanel.gameObject.SetActive(false);
    }

    public void AllShow()
    {
        ketteiButton.gameObject.SetActive(true);
        GodSkillDisPanel.gameObject.SetActive(true);
    }
}
