using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class FusionUI : MonoBehaviour
{
    public static FusionUI Instance { get; private set; }
    [SerializeField] private Button GodFusionYESButton;
    [SerializeField] private Button GodFusionNOButton;
    [SerializeField] private Transform GodUIButtonPanel;
    [SerializeField] private Button NewGodButton;
    [SerializeField] private Button GodUIButton;

    private int PushID = -1;//押されたボタンが持ってたID

    private GodData newGodData;

    private System.Action<bool, int, GodData> onDecision; // true=YES, false=NO

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        GodFusionYESButton.onClick.AddListener(() => OnDecision(true));
        GodFusionNOButton.onClick.AddListener(()=> OnDecision(false));
    }
    
    void Start()
    {
        AllHide();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show(List<GodData> GodFusions, GodData newGod, System.Action<bool, int, GodData> callback)
    {
        foreach (var god in GodFusions)
        {
            Button goduibutton = Instantiate(GodUIButton, GodUIButtonPanel);

            Image image = goduibutton.GetComponent<Image>();//////////////////////////////////////////////////画像を入れて、見た目等を良くしようと思う
            image.sprite = god.icon;
            //UIの見た目を変える処理等をする。
            goduibutton.onClick.AddListener(() => OnclickGodButton(god.id, god));//誰をクリックしたかの情報取れる

        }
        //NewGodButton.GetComponent<Text>().text = newGod.godName;
        //あたらしい方の神の力のUIもここで変える。
        Image image1 = NewGodButton.GetComponent<Image>();
        image1.sprite = newGod.icon;

        PushID = -1;
        newGodData = null;
        onDecision = null;
        onDecision = callback;
        AllShow();
    }

    public void OnDecision(bool isYes)
    {
        if (isYes && PushID == -1) return;
        if (!isYes) PushID = -1;
        foreach (Transform child in GodUIButtonPanel)
            Destroy(child.gameObject);
        
        AllHide();
        onDecision?.Invoke(isYes, PushID, newGodData);
    }

    public void AllShow()
    {
        GodUIButtonPanel.gameObject.SetActive(true);
        NewGodButton.gameObject.SetActive(true);
        GodFusionYESButton.gameObject.SetActive(true);
        GodFusionNOButton.gameObject.SetActive(true);
    }

    public void AllHide()
    {
        GodUIButtonPanel.gameObject.SetActive(false);
        NewGodButton.gameObject.SetActive(false);
        GodFusionYESButton.gameObject.SetActive(false);
        GodFusionNOButton.gameObject.SetActive(false);
    }
    
    private void OnclickGodButton(int godid, GodData god)
    {   //とりあえず、融合候補たちのボタンのイベントを設置
        //押されたときにスキルの表示をやる
        newGodData = god;
        PushID = godid;
        Debug.Log(PushID);
        
    }
}
