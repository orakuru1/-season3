using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class craftmode : MonoBehaviour
{
    public static craftmode instance;

    [Header("パネル")]
    public RectTransform craftArea;  //合成スロット用パネル
    public RectTransform inventoryArea;  //持ち物一覧パネル
    public RectTransform bukiArea;  //武器一覧パネル
    public RectTransform bougArea;  //防具一覧パネル

    [Header("ボタン")]
    public Button craftToggleButton;  //合成モード切替ボタン

    [Header("Inventory ScrollView")]
    public RectTransform inventoryConten;  //content
    public RectTransform bukiConten;
    public RectTransform bouguConten;
    public RectTransform inventoryViewport;  //viewportのrecttransform
    public RectTransform bukiViewport;
    public RectTransform bouguViewport;
    public float contentHeight = 1000;  //一番長いときに合わせたたかさ　

    public bool isCraftMode = false;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        craftToggleButton.onClick.AddListener(ToggleCraftMode);

        //初期状態から通常表示
        SetNormalModeUI();   
    }

    void ToggleCraftMode()
    {
        if(isCraftMode)
        {
            SetNormalModeUI();
        }
        else
        {
            SetCraftModeUI();
        }
    }

    void ResizeViewportHeight(float scale)
    {
        if(inventoryViewport == null || bukiViewport == null || bouguViewport == null) return;

        inventoryViewport.localScale = new Vector3(1f, scale, 1f);
        bukiViewport.localScale = new Vector3(1f, scale, 1f);
        bouguViewport.localScale = new Vector3(1f, scale, 1f);
    }

    void SetRectFull(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;  //通常モードは高さしていない
    }

    void SetRectTopHalf(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    void SetRectBottomHalf(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0.7f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    void SetNormalModeUI()
    {
        isCraftMode = false;

        //CraftAreaを非表示
        craftArea.gameObject.SetActive(false);
       

        //全画面
        SetRectFull(inventoryArea);
        SetRectFull(bukiArea);
        SetRectFull(bougArea);

        //スケールを戻す
        inventoryArea.localScale = Vector3.one;
        bukiArea.localScale = Vector3.one;
        bougArea.localScale = Vector3.one;
        craftArea.localScale = Vector3.one;
       

        //高さリセット
        inventoryArea.sizeDelta = Vector2.zero;
        bukiArea.sizeDelta = Vector2.zero;
        bougArea.sizeDelta = Vector2.zero;
        craftArea.sizeDelta = Vector2.zero;
      

        //contentは常に一定サイズ
        inventoryConten.sizeDelta = new Vector2(inventoryConten.sizeDelta.x, contentHeight);
        bukiConten.sizeDelta = new Vector2(bukiConten.sizeDelta.x, contentHeight);
        bouguConten.sizeDelta = new Vector2(bouguConten.sizeDelta.x, contentHeight);
        

        //viewportの高さを親に合わせる
        ResizeViewportHeight(1f);
    }

    void SetCraftModeUI()
    {
        isCraftMode = true;

        //CraftAreaを表示
        craftArea.gameObject.SetActive(true);

        //上半分
        SetRectTopHalf(craftArea);
        

        //下半分
        SetRectBottomHalf(inventoryArea);
        SetRectBottomHalf(bukiArea);
        SetRectBottomHalf(bougArea);

        //スケールを縮小
        inventoryArea.localScale = new Vector3(1f, 0.5f, 1f);
        bukiArea.localScale = new Vector3(1f, 0.5f, 1f);
        bougArea.localScale = new Vector3(1f, 0.5f, 1f);

        //高さを調整
        inventoryArea.sizeDelta = new Vector2(0, 30);
        bukiArea.sizeDelta = new Vector2(0, 30);
        bougArea.sizeDelta = new Vector2(0, 30);

        //contentの高さは固定
        inventoryConten.sizeDelta = new Vector2(inventoryConten.sizeDelta.x, contentHeight);
        bukiConten.sizeDelta = new Vector2(bukiConten.sizeDelta.x, contentHeight);
        bouguConten.sizeDelta = new Vector2(bouguConten.sizeDelta.x, contentHeight);


        //viewportの高さを親に合わせる
        ResizeViewportHeight(2.0f);

       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
