using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation : MonoBehaviour
{
    public Animator animator; //AnimatorをInspectorでアタッチ
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
        }
    }

    //プレイヤーが近くにいるかチェック（簡易的な距離判定）
    bool IsPlayerNearby()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player == null) return false;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        return distance < 3.0f; //3m以内ならOK
    }
}
