using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation : MonoBehaviour
{
    public Animator animator; //AnimatorをInspectorでアタッチ
    public Transform player; //プレイヤーを指定
    public string itemName = "Sphere"; //宝箱の中身
    public float itemGetDelay = 1.5f; //アニメーション終了から取得するまでの時間

    private bool isOpened = false; //一度だけ開く
    

    // Start is called before the first frame update
    void Start()
    {
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
        return distance < 1.0f; //3m以内ならOK
    }

    //アニメーション後にアイテム取得処理
    IEnumerator GetItemAfterDelay()
    {
        yield return new WaitForSeconds(itemGetDelay); //アニメーション終了待ち

        if(ItemUIManager.instance != null)
        {
            Debug.Log($"{itemName}を取得しました");
            ItemUIManager.instance.AddItem(itemName);
        }
        else
        {
            Debug.Log("ItemUImanagerがシーン内に見つかりません!");
        }
    }
}
