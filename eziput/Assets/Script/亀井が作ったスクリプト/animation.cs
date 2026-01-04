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

    [Header("ランダムアイテム設定")]
    public List<string> itemList = new List<string>(){"薬草", "パン", "木の枝", "神の腰布"};
    public List<ItemType> itemTypeList = new List<ItemType>(){ItemType.Item, ItemType.Item, ItemType.Weapon, ItemType.Armor};

    private string itemName; //選ばれたアイテム名
    private ItemType itemType; //選ばれたタイプ

    public enum ItemType{Item, Weapon, Armor}

    private bool isOpened = false; //一度だけ開く
    

    // Start is called before the first frame update
    void Start()
    {
        //開始時に中身をランダム決定
        int randomIndex = Random.Range(0, itemList.Count);
        itemName = itemList[randomIndex];
        itemType = itemTypeList[randomIndex];
        Debug.Log($"宝箱の中身は：{itemName}({itemType})");
        itemObject = this.gameObject;
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

            //一定時間後にアイテム取得
            StartCoroutine(GetItemAfterDelay());
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

        if(ItemUIManager.instance != null)
        {
            Debug.Log($"{itemName}を取得しました");

            //ポップアップ表示用のスプライト取得
            Sprite icon = null;
            if(ItemUIManager.instance.itemDataDict.ContainsKey(itemName))
            {
                icon = ItemUIManager.instance.itemDataDict[itemName].icon;
            }

            //取得ポップアップ表示
            if(GetItemPopUi.instance != null)
            {
                GetItemPopUi.instance.Show(itemName, icon);
            }
            
            switch (itemType)
            {
                case ItemType.Item:
                    ItemUIManager.instance.AddItem(itemName);
                    break;
                case ItemType.Weapon:
                    ItemUIManager.instance.AddWeapon(itemName);
                    break;
                case ItemType.Armor:
                    ItemUIManager.instance.AddArmor(itemName);
                    break;
            }
        }
        else
        {
            Debug.Log("ItemUImanagerがシーン内に見つかりません!");
        }

        //アイテムオブジェクトを消す
        if(itemObject != null)
        {
            itemObject.SetActive(false);
        }
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
        Debug.Log("宝箱にプレイヤー設定完了");
    }
}
