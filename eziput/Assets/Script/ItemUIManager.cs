using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager instance;

    [Header("各カテゴリのScrollview Content")]
    [SerializeField] private Transform itemcontentParent; // 持ち物用ScrollViewのContent
    [SerializeField] private Transform weaponcontentParent;  //武器
    [SerializeField] private Transform armorcontentParent;  //防具

    [Header("ボタンプレハブ全共通")]
    [SerializeField] private GameObject itemButtonPrefab; // アイテムボタンのプレハブ

    [Header("確認UI")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Text confirmText;

    private Dictionary<string, (int count, GameObject button)> itemDict = new();
    private Dictionary<string, (int count, GameObject button)> WeaponDict = new();
    private Dictionary<string, (int count, GameObject button)> armorDict = new();

    private GameObject selectedButton;
    private string selectedItemName;
    private string selectedCategory;

    

    private void Awake()
    {
        instance = this;
    }

    //=====================
    // アイテム追加処理
    //=====================
    public void AddItem(string itemName, string category)
    {
        Debug.Log($"{itemName}");
        switch (category.ToLower())
        {
            case "item":
            case "持ち物":
                addToCategory(itemName, itemDict, itemcontentParent);
                break;
            case "weapon":
            case "武器":
                addToCategory(itemName, WeaponDict, weaponcontentParent);
                break;
            case "armor":
            case "防具":
                addToCategory(itemName, armorDict, armorcontentParent);
                break;
            default:
                Debug.Log($"不明なカテゴリです: {category}");
                break;
        }
    }

    public void AddItem(string itemName)
    {
        addToCategory(itemName, itemDict, itemcontentParent);
    }

    public void AddWeapon(string itemName)
    {
        addToCategory(itemName, WeaponDict, weaponcontentParent);
    }

    public void AddArmor(string itemName)
    {
        addToCategory(itemName, armorDict, armorcontentParent);
    }

    //カテゴリに応じてアイテムを追加
    private void addToCategory(string itemName, Dictionary<string, (int count, GameObject button)> dict, Transform parent)
    {
        if(dict.ContainsKey(itemName))
        {
            //同じアイテムが既にある場合カウントアップ
            var data = dict[itemName];
            if(data.button == null)
            {
                //ボタンが削除されていたら再生成
                GameObject newButton = CreateItemButton(itemName, parent);
                dict[itemName] = (1, newButton);
            }
            else
            {
                data.count++;
                dict[itemName] = data;
                UpdateItemCountText(data.button, data.count);
            }
        }
        else
        {
            //新規アイテム
            GameObject newButton = CreateItemButton(itemName, parent);
            dict[itemName] = (1, newButton);
        }
    }

    //=====================
    // ボタン作成
    //=====================
    private GameObject CreateItemButton(string itemName, Transform parent)
    {
        Debug.Log($"{itemName}");
        GameObject newButton = Instantiate(itemButtonPrefab, parent);
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
            label.text = itemName;
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
        selectedCategory = GetItemCategory(itemName);

        confirmText.text = $"{itemName}を使いますか？";
        confirmPanel.SetActive(true);
    }

    //=====================
    // 使用確認の「はい」ボタンなどで呼ぶ処理
    //=====================
    public void UseSelectedItem(string itemName, string category)
    {
       Dictionary<string, (int count, GameObject button)> dict = GetDictionaryByCategory(category);
       if(!dict.ContainsKey(itemName)) return;

       var data = dict[itemName];
       data.count--;

       if(data.count <= 0)
       {
        Destroy(data.button);
        dict[itemName] = (0, null);
       }
       else
       {
        dict[itemName] = data;
        UpdateItemCountText(data.button, data.count);
       }
    }

    public void OnConfirmYes()
    {
        Debug.Log($"{selectedItemName}を使用しました！！");
        UseSelectedItem(selectedItemName, selectedCategory);
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

    //アイテムが属するカテゴリを判定
    private string GetItemCategory(string itemName)
    {
        if(itemDict.ContainsKey(itemName)) return "item";
        if(WeaponDict.ContainsKey(itemName)) return "weapon";
        if(armorDict.ContainsKey(itemName)) return "armor";
        return null;
    }

    //カテゴリ名から辞書を取得
    private Dictionary<string, (int count, GameObject button)> GetDictionaryByCategory(string category)
    {
        return category switch
        {
            "item" => itemDict,
            "weapon" => WeaponDict,
            "armor" => armorDict,
            _ => null
        };
    }
}
