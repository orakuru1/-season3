using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation : MonoBehaviour
{
    [Header("基本設定")]
    public Animator animator; //AnimatorをInspectorでアタッチ
    public Transform player; //プレイヤーを指定
    public GameObject itemObject;
    public float itemGetDelay = 1.5f; //アニメーション終了から取得するまでの時間

    [Header("確定アイテム設定")]
    public bool hasGuarateedItem = true; //確定アイテムを入れるか
    public List<string> guaranteedItemName = new List<string>();    //確定アイテム名
    public List<ItemType> guaranteedItemType = new List<ItemType>();  //確定アイテムタイプ
    

    [Header("ランダムアイテム設定")]
    public List<string> itemList = new List<string>(){"薬草", "パン", "木の枝", "神の腰布"};
    public List<ItemType> itemTypeList = new List<ItemType>(){ItemType.Item, ItemType.Item, ItemType.Weapon, ItemType.Armor};

    private bool isGuaranteedChest = false;
    private string dropItemName;
    private ItemType dropItemType;

    public enum ItemType{Item, Weapon, Armor}

    private bool isOpened = false; //一度だけ開く
    

    // Start is called before the first frame update
    void Start()
    {
       itemObject = this.gameObject;

       //ステージでまだ確定宝箱が決まっていない場合だけ抽選
       if(hasGuarateedItem && GameManager.Instance.TryAssignGuaranteedChest(this))
       {
        isGuaranteedChest = true;
        Debug.Log("この宝箱は[確定宝箱]です");
       }
       else
        {
            Debug.Log("この宝箱は[ランダム宝箱]です");
        }
        //audio = GetComponent<AudioSource>(); //SE追加
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーが近くにいてFキーを押したら開く
        if(Input.GetKeyDown(KeyCode.F) && !isOpened && IsPlayerNearby())
        {
            Debug.Log("アニメーション再生！！");
            animator.SetTrigger("open");
            isOpened = true;

            DecideItem();

            //一定時間後にアイテム取得
            StartCoroutine(GetItemAfterDelay());
        }
    }

    void DecideItem()
    {
        //確定アイテム
       if (isGuaranteedChest)
       {
            int gIndex = Random.Range(0, guaranteedItemName.Count);
            dropItemName = guaranteedItemName[gIndex];
            dropItemType = guaranteedItemType[gIndex];

            Debug.Log($"確定アイテム{dropItemName}");
       }
       else
       {
            //ランダムアイテム
            int randomIndex = Random.Range(0, itemList.Count);
            dropItemName = itemList[randomIndex];
            dropItemType = itemTypeList[randomIndex];

            Debug.Log($"ランダムアイテム：{itemList[randomIndex]}({itemTypeList[randomIndex]})");
            Debug.Log("確定アイテムではない");
       }


    }

    //プレイヤーが近くにいるかチェック（簡易的な距離判定）
    bool IsPlayerNearby()
    {
        if(player == null) return false;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        return distance < 2.0f; //3m以内ならOK
    }

    //アニメーション後にアイテム取得処理
    IEnumerator GetItemAfterDelay()
    {
        yield return new WaitForSeconds(itemGetDelay); //アニメーション終了待ち

        if(ItemUIManager.instance == null)
        {
            Debug.Log("ItemUIManagerが見つかりません");
            yield break;
        }

        Sprite icon = null;
        if (ItemUIManager.instance.itemDataDict.ContainsKey(dropItemName))
        {
            icon = ItemUIManager.instance.itemDataDict[dropItemName].icon;
        }

        if (LogManager.Instance != null)
        {
            LogManager.Instance.AddItemLog($"{LogManager.ColorText(dropItemName, "#4444FF")} を習得した！", icon);
        }

        switch (dropItemType)
        {
            case ItemType.Item:
                ItemUIManager.instance.AddItem(dropItemName);
                Debug.Log("Itemに追加");
                break;
            case ItemType.Weapon:
                ItemUIManager.instance.AddWeapon(dropItemName);
                break;
            case ItemType.Armor:
                ItemUIManager.instance.AddArmor(dropItemName);
                break;
        }

        itemObject.SetActive(false);
    }
    private void OnEnable()
    {
        GameManager.OnPlayerSpawned += setPlayer;
    }
    private void OnDisable()
    {
        GameManager.OnPlayerSpawned -= setPlayer;
    }
    public void setPlayer(GameObject p)
    {
        player = p.transform;
        //Debug.Log("宝箱にプレイヤー設定完了");
    }

    public void SetGuaranteedItem(string name, ItemType type)
    {
        isGuaranteedChest = true;
        dropItemName = name;
        dropItemType = type;

        Debug.Log($"確定宝箱: {name}");
    }
}
