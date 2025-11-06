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

    [Header("装備スロット（左側）")]
    [SerializeField] private Transform weaponSlotParent;
    [SerializeField] private Transform armorSlotParent;

    [Header("ステータス画面表示用")]
    [SerializeField] private Image weaponStatusImage;
    [SerializeField] private Image armorStatusImage;

    [Header("アイテム画像登録用")]
    public Sprite potionSprite;
    public Sprite kosinunoSprite;
    public Sprite panSprite;
    public Sprite bouSprite;
    public Sprite kinobouSprite;

    private Dictionary<string, (int count, GameObject button)> itemDict = new(); //持ち物用辞書
    private Dictionary<string, (int count, GameObject button)> WeaponDict = new(); //武器用辞書
    private Dictionary<string, (int count, GameObject button)> armorDict = new(); //防具用辞書
    
    public Dictionary<string, Sprite> itemSpriteDict = new Dictionary<string, Sprite>(); //アイテム画像用辞書

    private GameObject selectedButton;
    private string selectedItemName;
    private string selectedCategory;

    private GameObject equippedWeapon;
    private GameObject equippedArmor;

    private GameObject equippedWeaponButton;
    private GameObject equippedArmorButton;

    

    private void Awake()
    {
        instance = this;

        //名前と画像を辞書に登録
        itemSpriteDict["ポーション"] = potionSprite;
        itemSpriteDict["bou"] = bouSprite;
        itemSpriteDict["木の枝"] = kinobouSprite;
        itemSpriteDict["神の腰布"] = kosinunoSprite;
        itemSpriteDict["パン"] = panSprite;
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
            label.text = $"{itemName}";
        }

        //画像設定
        Image icon = newButton.GetComponentInChildren<Image>();
        if(icon != null && itemSpriteDict.ContainsKey(itemName))
        {
            icon.sprite = itemSpriteDict[itemName];
        }
        else
        {
            Debug.Log($"画像が登録されていないアイテム: {itemName}");
        }

        Button btn = newButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        // ボタンを渡す形でイベント登録
        GameObject btnCopy = newButton;
        btn.onClick.AddListener(() => OnItemButtonClicked(btnCopy));

        return newButton;
    }

    private Transform GetParentByCategory(string itemName)
    {
        if(itemName.Contains("剣") || itemName.Contains("武器")) return weaponcontentParent;
        if(itemName.Contains("神の腰布") || itemName.Contains("防具")) return armorcontentParent;
        return itemcontentParent;
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

        //すでに装備中なら何もしない
        if((selectedCategory == "weapon" && button == equippedWeaponButton) || (selectedCategory == "armor" && button == equippedArmorButton))
        {
            Debug.Log($"{itemName}はすでに装備中です。");
            return;
        }
        
        if(selectedCategory == "item")
        {
            confirmText.text = $"{itemName}を使いますか？";
        }
        else if(selectedCategory == "weapon"  || selectedCategory == "armor")
        {
            confirmText.text = $"{itemName}を装備しますか？";
        }
        
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
        if(selectedCategory == "item")
        {
            Debug.Log($"{selectedItemName}を使用しました！！");
            UseSelectedItem(selectedItemName, selectedCategory);
            Destroy(selectedButton);
        }
        else if(selectedCategory == "weapon" || selectedCategory == "armor")
        {
            Debug.Log($"{selectedItemName}を装備しました！");
            EquidItem(selectedButton, selectedCategory);
        }
        
        confirmPanel.SetActive(false);
        //↓アイテムの効果を発動させる処理
    }

    public void OnConfirmNo()
    {
        confirmPanel.SetActive(false);
    }

    //装備処理
    private void EquidItem(GameObject button, string category)
    {
        if(category == "weapon")
        {
            if(equippedWeapon != null)
            {
                //ReturnToList(equippedWeapon, weaponcontentParent);
                SetEquippedVisual(equippedWeapon, false);
            }

            equippedWeaponButton = selectedButton;
            equippedWeapon = button; //新しい武器を装備
            button.transform.SetSiblingIndex(0); //一番左に移動
            SetEquippedVisual(button, true); //見た目変更
            //MoveToSlot(button, weaponSlotParent);

            Image icon = button.GetComponentInChildren<Image>();
            if(icon != null && weaponStatusImage != null)
            {
               weaponStatusImage.sprite = icon.sprite;
            }
        }
        else if(category == "armor")
        {
            if(equippedArmor != null)
            {
                //ReturnToList(equippdArmor, armorcontentParent);
                SetEquippedVisual(equippedArmor, false);
            }

            equippedArmorButton = selectedButton;
            equippedArmor = button; //新しい武器を装備
            button.transform.SetSiblingIndex(0);  //一番左に移動
            SetEquippedVisual(button, true); //見た目変更
            //MoveToSlot(button, armorSlotParent);

            Image icon = button.GetComponentInChildren<Image>();
            if(icon != null && armorStatusImage != null)
            {
                armorStatusImage.sprite = icon.sprite;
            }
        }
    }

    //装備ボタンを左側に移動
    private void MoveToSlot(GameObject button, Transform slotParent)
    {
        button.transform.SetParent(slotParent);
        button.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    //リストに戻す
    /*private void ReturnToList(GameObject button, Transform listParent)
    {
        button.transform.SetParent(listParent);
    }*/

    private void SetEquippedVisual(GameObject button, bool equipped)
    {
        Image bg = button.GetComponent<Image>();
        if(bg != null)
        {
            bg.color = equipped ? new Color(0.8f, 0.8f, 1f) : Color.white;
        }
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
