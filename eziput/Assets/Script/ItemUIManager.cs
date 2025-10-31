using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager instance;

    [Header("UI設定")]
    [SerializeField] private Transform contentParent;      // ScrollView の Content
    [SerializeField] private GameObject itemButtonPrefab;  // アイテムボタンのプレハブ
    [SerializeField] private GameObject confirmPanel; //[使いますか？]のパネル
    [SerializeField] private Text confirmText;  //確認メッセージ

    private string selectedItemName;  //選ばれたアイテム名

    // アイテムと個数を管理
    private Dictionary<string, (GameObject button, int count)> itemData 
        = new Dictionary<string, (GameObject, int)>();

    //アイテム名 Spriteの対応辞書
    [System.Serializable]
    public class ItemSpriteData
    {
        public string itemName;
        public Sprite itemSprite;
    }
    [SerializeField] private List<ItemSpriteData> itemSpriteList = new List<ItemSpriteData>();

    private void Awake()
    {
        instance = this;
        // ゲーム開始時にリセット（重複防止）
        itemData.Clear();
    }

    /// <summary>
    /// アイテム追加処理
    /// </summary>
    public void AddItem(string itemName)
    {
        // すでに持っている場合：カウントアップのみ
        if (itemData.ContainsKey(itemName))
        {
            var data = itemData[itemName];
            data.count++;
            itemData[itemName] = data;

            // テキスト更新（CountText を想定）
            Text existinglabel = data.button.GetComponentInChildren<Text>();
            if(existinglabel != null)
            {
                existinglabel.text = $"{itemName} ×{data.count}";
            }
            
            return;
        }

        // 初めてのアイテムなら新規生成
        GameObject newButton = Instantiate(itemButtonPrefab, contentParent);
        newButton.name = itemName;

        //テキスト設定
        Text newlabel = newButton.GetComponentInChildren<Text>();
        if(newlabel != null)
        {
            newlabel.text = itemName;
        }
        
        //アイコン設定
        Image icon = newButton.GetComponentInChildren<Image>();
        if(icon != null)
        {
            icon.sprite = GetSpriteByName(itemName);
        }
        

        // データ登録
        itemData[itemName] = (newButton, 1);

        // ボタンクリック処理
        Button btn = newButton.GetComponent<Button>();
        btn.onClick.AddListener(() => OnItemButtonClicked(itemName));
    }

    //Sprite取得関数
    private Sprite GetSpriteByName(string itemName)
    {
    foreach (var data in itemSpriteList)
    {
        if (data.itemName == itemName)
            return data.itemSprite;
    }

    // 見つからなかった場合、デフォルト画像（nullでもOK）
    return null;
    }

    /// <summary>
    /// ボタン押下時処理
    /// </summary>
    private void OnItemButtonClicked(string itemName)
    {
        Debug.Log($"「{itemName}」を選択しました！");
        selectedItemName = itemName;
        confirmText.text = $"{itemName}を使いますか？";
        confirmPanel.SetActive(true);
        // 装備・使用などのUI呼び出しはここに
    }

    //はいボタン
    public void OnConfirmYes()
    {
        Debug.Log($"{selectedItemName}を使用しました");
        confirmPanel.SetActive(false);
    }

    //いいえボタン
    public void OnConfirmNO()
    {
        confirmPanel.SetActive(false);
    }

}
