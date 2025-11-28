using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager instance;
#region 変数
    

    [Header("各カテゴリのScrollView Content")]
    [SerializeField] private Transform itemContentParent;     // 持ち物
    [SerializeField] private Transform weaponContentParent;   // 武器
    [SerializeField] private Transform armorContentParent;    // 防具

    [Header("ボタンプレハブ共通")]
    [SerializeField] private GameObject itemButtonPrefab;

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
    public Sprite tateSprite;
    public Sprite panSprite;
    public Sprite bouSprite;
    public Sprite kinobouSprite;
    public Sprite tetuSprite;
    public Sprite isihen;
    public Sprite isiduti;
    public Sprite nuno;
    public Sprite suna;
    public Sprite sunanuno;
    public Sprite taimatu;
    public Sprite tosuto;
    public Sprite doromizu;
    public Sprite mizu;
    public Sprite hone;
    public Sprite himo;
    public Sprite honenaihu;

    [Header("合成スロットUI")]
    [SerializeField] private List<Image> craftSlots = new List<Image>();

    //選択された素材名リスト
    private List<string> craftItems = new List<string>();

    [SerializeField] private Button craftExecuteButton;

    // 各カテゴリの辞書
    private Dictionary<string, (int count, GameObject button)> itemDict = new();
    private Dictionary<string, (int count, GameObject button)> weaponDict = new();
    private Dictionary<string, (int count, GameObject button)> armorDict = new();

    // アイテム名と画像の対応
    public Dictionary<string, Sprite> itemSpriteDict = new();

    //合成レシピ
    private Dictionary<string, List<(string itemName, int count)>> resipeDict = new Dictionary<string, List<(string itemName, int count)>>();

    // 選択・装備管理
    private GameObject selectedButton;
    private string selectedItemName;
    private string selectedCategory;

    private GameObject equippedWeapon;
    private GameObject equippedArmor;
    private GameObject equippedWeaponButton;
    private GameObject equippedArmorButton;

    //合成スロットに入っているアイテム名
    private string craftItem1 = null;
    private string craftItem2 = null;

    private bool isUnEquipping = false;
#endregion

    //=============================
    // 初期化
    //=============================
    private void Awake()
    {
        instance = this;

        // 名前と画像を辞書に登録
        itemSpriteDict["薬草"] = potionSprite;
        itemSpriteDict["bou"] = bouSprite;
        itemSpriteDict["木棒"] = kinobouSprite;
        itemSpriteDict["神の腰布"] = kosinunoSprite;
        itemSpriteDict["鋼鉄"] = tateSprite;
        itemSpriteDict["パン"] = panSprite;
        itemSpriteDict["鉄"] = tetuSprite;
        itemSpriteDict["石片"] = isihen;
        itemSpriteDict["石槌"] = isiduti;
        itemSpriteDict["布"] = nuno;
        itemSpriteDict["砂"] = suna;
        itemSpriteDict["砂包帯"] = sunanuno;
        itemSpriteDict["松明"] = taimatu;
        itemSpriteDict["トースト"] = tosuto;
        itemSpriteDict["濁った水"] = doromizu;
        itemSpriteDict["水"] = mizu;
        itemSpriteDict["骨"] = hone;
        itemSpriteDict["紐"] = himo;
        itemSpriteDict["骨ナイフ"] = honenaihu;

        RegisterRecipes();
    }

    private void Start()
    {
        //合成ボタンが押されたときに合成処理
        craftExecuteButton.onClick.AddListener(OnCraftExecute);
    }
#region 合成レシピとカテゴリ
    //アイテムのカテゴリ固定辞書
    private Dictionary<string, string> baseCategoryDict = new()
    {
        {"薬草", "item"},
        {"パン", "item"},
        {"布", "item"},
        {"砂", "item"},
        {"砂包帯", "item"},
        {"濁った水", "item"},
        {"水", "item"},
        {"紐", "item"},
        {"木棒", "weapon"},
        {"石槌", "weapon"},
        {"骨", "weapon"},
        {"骨ナイフ", "weapon"},
        {"神の腰布", "armor"},
        {"石片", "item"},
        {"鉄", "item"},
        {"bou", "item"},
        {"鋼鉄", "armor"},
        {"トースト", "item"},
        {"松明", "item"}
    };

    //合成レシピ
    private void RegisterRecipes()
    {
        resipeDict["砂包帯"] = new List<(string, int)>()
        {
            ("布", 1),
            ("砂", 1)
        };
        
        resipeDict["水"] = new List<(string, int)>()
        {
            ("布", 1),
            ("濁った水", 1)
        };

        resipeDict["石槌"] = new List<(string, int)>()
        {
            ("木棒", 1),
            ("石片", 1),
            ("紐", 1)
        };

        resipeDict["トースト"] = new List<(string, int)>()
        {
            ("松明", 1),
            ("パン", 1)
        };

        resipeDict["骨ナイフ"] = new List<(string, int)>()
        {
            ("紐", 1),
            ("骨", 1)
        };
    }
#endregion
#region アイテム種類とカテゴリ

    //=============================
    // アイテム追加処理
    //=============================
    public void AddItem(string itemName, string category)
    {
        Debug.Log($"{itemName}を追加");

        switch (category.ToLower())
        {
            case "item":
            case "持ち物":
                AddToCategory(itemName, itemDict, itemContentParent);
                break;

            case "weapon":
            case "武器":
                AddToCategory(itemName, weaponDict, weaponContentParent);
                break;

            case "armor":
            case "防具":
                AddToCategory(itemName, armorDict, armorContentParent);
                break;

            default:
                Debug.LogWarning($"不明なカテゴリ: {category}");
                break;
        }
    }

    public void AddItem(string itemName) => AddToCategory(itemName, itemDict, itemContentParent);
    public void AddWeapon(string itemName) => AddToCategory(itemName, weaponDict, weaponContentParent);
    public void AddArmor(string itemName) => AddToCategory(itemName, armorDict, armorContentParent);


    //=============================
    // カテゴリ別アイテム追加
    //=============================
    private void AddToCategory(string itemName, Dictionary<string, (int count, GameObject button)> dict, Transform parent)
    {
        if (dict.ContainsKey(itemName))
        {
            var data = dict[itemName];
            if (data.button == null)
            {
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
            GameObject newButton = CreateItemButton(itemName, parent);
            dict[itemName] = (1, newButton);
        }
    }

#endregion
#region ボタン生成と押下処理
    

    //=============================
    // ボタン生成処理
    //=============================
    private GameObject CreateItemButton(string itemName, Transform parent)
    {
        GameObject newButton = Instantiate(itemButtonPrefab, parent);
        newButton.name = itemName;

        // テキスト設定
        Text label = newButton.GetComponentInChildren<Text>(true);
        if (label != null) label.text = itemName;

        // 画像設定
        Image icon = newButton.GetComponentInChildren<Image>();
        if (icon != null && itemSpriteDict.ContainsKey(itemName))
            icon.sprite = itemSpriteDict[itemName];
        else
            Debug.LogWarning($"画像が登録されていないアイテム: {itemName}");

        // クリックイベント登録
        Button btn = newButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnItemButtonClicked(newButton));

        return newButton;
    }


    //=============================
    // ボタン押下処理
    //=============================
    public void OnItemButtonClicked(GameObject button)
    {
        string itemName = button.name;

        //合成モードなら合成処理に分岐
        if(craftmode.instance.isCraftMode)
        {
            AddToCraftSlot(itemName);
            return;
        }
        selectedButton = button;
        selectedItemName = itemName;
        selectedCategory = GetItemCategory(itemName);

        if ((selectedCategory == "weapon" && button == equippedWeaponButton) ||
            (selectedCategory == "armor" && button == equippedArmorButton))
        {
            confirmText.text = $"{itemName}の装備を解除しますか？";
            confirmPanel.SetActive(true);
            
            //解除フラグを設定
            isUnEquipping = true;
            return;
        }

        isUnEquipping = false;
        if (selectedCategory == "item")
            confirmText.text = $"{itemName}を使いますか？";
        else
            confirmText.text = $"{itemName}を装備しますか？";

        confirmPanel.SetActive(true);
    }
#endregion
#region 合成関連
    //合成ボタンの処理
    public void OnCraftExecute()
    {
        if(craftItems.Count == 0)
        {
            Debug.Log("素材が選ばれていません!");
            return;
        }

        //レシピ検索
        string result = GetCraftResult(craftItems);
        if(result == null)
        {
            ClearCraftSlots();
            Debug.Log("この組み合わせでは合成できません");
            return;
        }

        //素材消費
        foreach(string item in craftItems)
            UseSelectedItem(item, GetItemCategory(item));

        
        //アイテム追加
        string resultCategory = GetItemCategory(result);

        //正しいカテゴリへ追加
        if(resultCategory != null)
        {
            AddItem(result, resultCategory);
        }
        else
        {
            AddItem(result, "item");
        }
        Debug.Log($"合成成功!{result}");

        ClearCraftSlots();
    }

    private string GetCraftResult(List<string> materials)
    {
        //順番を無視して判定
        foreach(var recipe in resipeDict)
        {
            var req = recipe.Value;

            //レシピが2素材の前提
            if(req.Count != materials.Count) continue;

            bool match = true;

            foreach(var reqItem in req)
            {
                //必要数を満たしているかチェック
                int required = reqItem.count;
                int owned = materials.FindAll(x => x == reqItem.itemName).Count;

                if(owned < required)
                {
                    match = false;
                    break;
                }
            }

            if(match) return recipe.Key;
        }
        return null;
    }

    private void ClearCraftSlots()
    {
        craftItems.Clear();
        foreach (var slot in craftSlots)
        {
            slot.sprite = null;
            slot.color = new Color(255,255,255,255);
        }
    }


#endregion
    //=============================
    // 確認パネル（はい）
    //=============================
    public void OnConfirmYes()
    {
        //装備中フラグがON 装備解除
        if(isUnEquipping)
        {
            UnequipItem(selectedCategory);
            isUnEquipping = false;
            confirmPanel.SetActive(false);
            return;
        }

        if (selectedCategory == "item")
        {
            bool used = ActivateItemEffect(selectedItemName);
            if (used)
                UseSelectedItem(selectedItemName, selectedCategory);
        }
        else if (selectedCategory == "weapon" || selectedCategory == "armor")
        {
            EquidItem(selectedButton, selectedCategory);
        }

        confirmPanel.SetActive(false);
    }

    //=============================
    // 確認パネル（いいえ）
    //=============================
    public void OnConfirmNo() => confirmPanel.SetActive(false);

    //合成スロットにアイテム挿入
    private void AddToCraftSlot(string itemName)
    {
        string category = GetItemCategory(itemName);
        var dict = GetDictionaryByCategory(category);

        if(dict == null || !dict.ContainsKey(itemName))
        {
            Debug.Log($"{itemName}が辞書にありません");
            return;
        }

        int currentCount = dict[itemName].count;

        //所持数チェック
        int alreadyUsed = craftItems.FindAll(x => x == itemName).Count;

        if(currentCount - alreadyUsed <= 0)
        {
            Debug.Log($"{itemName}の所持数が不足しています");
            return;
        }

        //スロットが満杯か確認
        if(craftItems.Count >= craftSlots.Count)
        {
            Debug.Log("スロットがすべて埋まっています");
            return;
        }

        craftItems.Add(itemName);

        //UI反映
        craftSlots[craftItems.Count - 1].sprite = itemSpriteDict[itemName];
        craftSlots[craftItems.Count - 1].color = Color.white;
    }
#region アイテムの使用・装備効果
    //=============================
    // アイテム使用処理
    //=============================
    private bool ActivateItemEffect(string itemName)
    {
        PlayerUnit player = FindObjectOfType<PlayerUnit>();
        if (player == null)
        {
            Debug.LogError("プレイヤーが見つかりません!");
            return false;
        }

        switch (itemName)
        {
            case "水":
                if (player.status.currentHP >= player.status.maxHP) return false;
                player.Heal(5);
                return true;
            
            case "薬草":
                if (player.status.currentHP >= player.status.maxHP) return false;
                player.Heal(20);
                return true;

            case "パン":
                if (player.status.currentHP >= player.status.maxHP) return false;
                player.Heal(10);
                return true;
            
            case "トースト":
                if(player.status.currentHP >= player.status.maxHP) return false;
                player.Heal(30);
                return true;
            
            case "砂包帯":
                if(player.status.currentHP >= player.status.maxHP) return false;
                player.Heal(5);
                player.equipDefenseBonus += 3;
                return true;
            
            default:
                Debug.Log($"{itemName}は合成素材のため使えません!");
                return false;
        }
    }

    //=============================
    // 使用したアイテムの減少処理
    //=============================
    public void UseSelectedItem(string itemName, string category)
    {
        var dict = GetDictionaryByCategory(category);
        if (!dict.ContainsKey(itemName)) return;

        var data = dict[itemName];
        data.count--;

        if (data.count <= 0)
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

    //=============================
    // 装備処理
    //=============================
    private void EquidItem(GameObject button, string category)
    {
        PlayerUnit player = FindObjectOfType<PlayerUnit>();
        if (player == null) return;

        // 武器装備
        if (category == "weapon")
        {
            if (equippedWeapon != null)
                SetEquippedVisual(equippedWeapon, false);

            equippedWeaponButton = selectedButton;
            equippedWeapon = button;

            button.transform.SetSiblingIndex(0);
            SetEquippedVisual(button, true);

            Image icon = button.GetComponentInChildren<Image>();
            if (icon != null && weaponStatusImage != null)
                weaponStatusImage.sprite = icon.sprite;

            int atkBonus = selectedItemName switch
            {
                "木棒" => 3,
                "石槌" => 5,
                "骨"   => 2,
                "骨ナイフ" => 7,
                _ => 0
            };

            player.equidpAttackBonus = atkBonus;

            GetStatus statusUI = FindObjectOfType<GetStatus>();
            if (statusUI != null)
            {
                statusUI.equippedWeaponName = selectedItemName;
                statusUI.UpdateStatus();
            }

            Debug.Log($"{selectedItemName}を装備！（攻撃 +{atkBonus}）");
        }

        // 防具装備
        else if (category == "armor")
        {
            if (equippedArmor != null)
                SetEquippedVisual(equippedArmor, false);

            equippedArmorButton = selectedButton;
            equippedArmor = button;

            button.transform.SetSiblingIndex(0);
            SetEquippedVisual(button, true);

            Image icon = button.GetComponentInChildren<Image>();
            if (icon != null && armorStatusImage != null)
                armorStatusImage.sprite = icon.sprite;

            int defBonus = selectedItemName switch
            {
                "神の腰布" => 5,
                "鋼鉄" => 10,
                _ => 0
            };

            player.equipDefenseBonus += defBonus;

            GetStatus statusUI = FindObjectOfType<GetStatus>();
            if (statusUI != null)
            {
                statusUI.equippedArmorName = selectedItemName;
                statusUI.UpdateStatus();
            }

            Debug.Log($"{selectedItemName}を装備！（防御 +{defBonus}）");
        }
    }

    //装備解除処理
    public void UnequipItem(string category)
    {
        PlayerUnit player = FindObjectOfType<PlayerUnit>();
        if(player == null) return;

        GetStatus statusUI = FindObjectOfType<GetStatus>();

        if(category == "weapon")
        {
            if(equippedWeaponButton == null) return;

            //見た目を戻す
            SetEquippedVisual(equippedWeaponButton, false);

            //装備スロット画像を消す
            if(weaponStatusImage != null)
            {
                weaponStatusImage.sprite = null;
            }

            //ステータスを元に戻す
            player.equidpAttackBonus = 0;

            //装備解除
            equippedWeaponButton = null;
            equippedWeapon = null;

            if(statusUI != null)
            {
                statusUI.equippedWeaponName = "なし";
            }

            //ステータス再描画
            FindObjectOfType<GetStatus>()?.UpdateStatus();
            
            Debug.Log("武器を解除しました");
        }
        else if(category == "armor")
        {
            if(equippedArmorButton == null) return;

            //見た目を戻す
            SetEquippedVisual(equippedArmorButton, false);

            //装備スロット画像を消す
            if(armorStatusImage != null)
            {
                armorStatusImage.sprite = null;
            }

            //ステータスを元に戻す
            player.equipDefenseBonus = 0;

            //装備解除
            equippedArmorButton = null;
            equippedArmor = null;

            if(statusUI != null)
            {
                statusUI.equippedArmorName = "なし";
            }

            //ステータス再描画
            FindObjectOfType<GetStatus>()?.UpdateStatus();
            
            Debug.Log("防具を解除しました");
        }
    }
#endregion

    //=============================
    // 見た目変更
    //=============================
    private void SetEquippedVisual(GameObject button, bool equipped)
    {
        Image bg = button.GetComponent<Image>();
        if (bg != null)
            bg.color = equipped ? new Color(0.8f, 0.8f, 1f) : Color.white;
    }


    //=============================
    // カウント更新
    //=============================
    private void UpdateItemCountText(GameObject button, int count)
    {
        Text countText = button.GetComponentInChildren<Text>(true);
        var dict = GetDictionaryByCategory(GetItemCategory(button.name));

        bool isHealItem = dict == itemDict;  //itemDictは持ち物
        string itemName = button.name;

        if(countText == null) return;

        if (isHealItem)
        {
            countText.text = (count > 1) ? $"{itemName}×{count}" : button.name;
        }
        else
        {
            countText.text = itemName;
        }
            
    }


    //=============================
    // カテゴリ取得
    //=============================
    private string GetItemCategory(string itemName)
    {
        if (itemDict.ContainsKey(itemName)) return "item";
        if (weaponDict.ContainsKey(itemName)) return "weapon";
        if (armorDict.ContainsKey(itemName)) return "armor";

        //未所持でもカテゴリを返せるように
        if(baseCategoryDict.ContainsKey(itemName))
           return baseCategoryDict[itemName];
        
        return null;
    }


    //=============================
    // 辞書取得
    //=============================
    private Dictionary<string, (int count, GameObject button)> GetDictionaryByCategory(string category)
    {
        return category switch
        {
            "item" => itemDict,
            "weapon" => weaponDict,
            "armor" => armorDict,
            _ => null
        };
    }
}
