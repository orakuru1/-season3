using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager instance;

    [SerializeField] private Transform contentParent; // ScrollViewのContent
    [SerializeField] private GameObject itemButtonPrefab; // アイテムボタンのプレハブ

    private Dictionary<string, (int count, GameObject button)> itemData = new Dictionary<string, (int, GameObject)>();
    private Dictionary<string, (int count, GameObject button)> itemDataDict = new Dictionary<string, (int count, GameObject button)>();

    [Header("確認UI")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Text confirmText;

    private GameObject selectedButton;
    private string selectedItemName;

    

    private void Awake()
    {
        instance = this;
    }

    //=====================
    // アイテム追加処理
    //=====================
    public void AddItem(string itemName)
    {
        Debug.Log($"{itemName}");
        // すでに持っているか確認
        if (itemData.ContainsKey(itemName))
        {
            // もし削除済み（ボタンが存在しない）なら再生成する
            if (itemData[itemName].button == null)
            {
                GameObject newButton = CreateItemButton(itemName);
                itemData[itemName] = (1, newButton);
            }
            else
            {
                // ボタンが残ってるならカウントを増やす
                var data = itemData[itemName];
                data.count++;
                itemData[itemName] = data;

                // 数量テキスト更新
                UpdateItemCountText(data.button, data.count);
            }
        }
        else
        {
            // 初取得：新しいボタンを生成
            GameObject newButton = CreateItemButton(itemName);
            itemData.Add(itemName, (1, newButton));
        }
    }

    //=====================
    // ボタン作成
    //=====================
    private GameObject CreateItemButton(string itemName)
    {
        Debug.Log($"{itemName}");
        GameObject newButton = Instantiate(itemButtonPrefab, contentParent);
        newButton.name = itemName;

        // ボタン内テキスト更新
        Text label = newButton.GetComponentInChildren<Text>(true);
        if(label == null)
        {
            //子階層に"Text"という名前のオブジェクトがあるなら
            Transform textTransform = newButton.transform.Find("Text");
            if(textTransform != null)
            {
                label = textTransform.GetComponent<Text>();
            }
        }
        if (label != null)
        {
            if(itemDataDict != null && itemDataDict.ContainsKey(itemName))
            {
                var itemData = itemDataDict[itemName];
                label.text = $"{itemName}×{itemData.count}";
            }
            else
            {
                label.text = itemName; //初回取得時
            }
            
        }

        Button btn = newButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        // ボタンを渡す形でイベント登録
        GameObject btnCopy = newButton;
        btn.onClick.AddListener(() => OnItemButtonClicked(btnCopy));

        return newButton;
    }

    //=====================
    // ボタン押下処理
    //=====================
    public void OnItemButtonClicked(GameObject button)
    {
        string itemName = button.name;
        selectedButton = button;
        selectedItemName = itemName;
        confirmText.text = $"{itemName}を使いますか？";
        confirmPanel.SetActive(true);
    }

    //=====================
    // 使用確認の「はい」ボタンなどで呼ぶ処理
    //=====================
    public void UseSelectedItem()
    {
        if (string.IsNullOrEmpty(selectedItemName)) return;

        if (itemData.ContainsKey(selectedItemName))
        {
            var data = itemData[selectedItemName];
            data.count--;

            if (data.count <= 0)
            {
                // ボタンを削除（後で再取得できるように null 登録）
                Destroy(data.button);
                itemData[selectedItemName] = (0, null);
            }
            else
            {
                // カウント減少を反映
                itemData[selectedItemName] = data;
                UpdateItemCountText(data.button, data.count);
            }
        }

        confirmPanel.SetActive(false);
    }

    public void OnConfirmYes()
    {
        Debug.Log($"{selectedItemName}を使用しました！！");
        Destroy(selectedButton);
        confirmPanel.SetActive(false);
    }

    public void OnConfirmNo()
    {
        confirmPanel.SetActive(false);
    }

    //=====================
    // 数量テキスト更新
    //=====================
    private void UpdateItemCountText(GameObject button, int count)
    {
        Text countText = button.transform.Find("CountText")?.GetComponent<Text>();
        if (countText != null)
        {
            countText.text = (count > 1) ? $"×{count}" : "";
        }
    }
}
