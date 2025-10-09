using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCanvas : MonoBehaviour
{
    [Header("ESCキーで開くパネル設定")]
    public GameObject panel;

    [Header("持ち物画面(Imageなど)")]
    public GameObject statusImage;  //ステータスイメージ
    public GameObject bukiImage;    //武器イメージ
    public GameObject bouguImage;   //防具イメージ
    public GameObject inventoryImage;

    [Header("プレイヤー操作スクリプト")]
    public MonoBehaviour playerController;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
        if(statusImage != null) 
           statusImage.SetActive(false);
        if(inventoryImage != null)
           inventoryImage.SetActive(false);
        if(bukiImage != null)
           bukiImage.SetActive(false);
        if(bouguImage != null)
           bouguImage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //ESCキーが押されたトグル
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("押されてない");
            TogglePanel();
        }
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
        SetPlayerControl(false);
    }

    public void HisPanel()
    {
        panel.SetActive(false);
        SetPlayerControl(false);
    }

    public void TogglePanel()
    {
        bool newState = !panel.activeSelf;

        if(newState)
        {
            panel.SetActive(true);
            statusImage.SetActive(true);
            if(inventoryImage != null)
               inventoryImage.SetActive(false);
            if(bukiImage != null)
               bukiImage.SetActive(false);

            SetPlayerControl(false);
        }
        else
        {
            panel.SetActive(false);
            SetPlayerControl(true);
        }
    }

    public void showInventory()
    {
        Debug.Log("押した");
        //ESCパネルを閉じて持ち物画面表示
        statusImage.SetActive(false);
        if(inventoryImage != null)
           inventoryImage.SetActive(true);
    }

    public void showbuki()
    {
        Debug.Log("押した");
        //ESCパネルを閉じて武器画面表示
        statusImage.SetActive(false);
        if(bukiImage != null)
           bukiImage.SetActive(true);
    }

    public void showbougu()
    {
        Debug.Log("押した");
        //ESCパネルを閉じて防具画面表示
        statusImage.SetActive(false);
        if(bouguImage != null)
           bouguImage.SetActive(true);
    }

    //持ち物画面から戻るボタンなどで呼ぶ
    public void CloseInventory()
    {
        if(inventoryImage != null) 
           inventoryImage.SetActive(false);
        
        if(bukiImage != null)
           bukiImage.SetActive(false);
        
        if(bouguImage != null)
           bouguImage.SetActive(false);
        
        panel.SetActive(true);
        statusImage.SetActive(true);
    }

    //プレイヤー操作を無効化する
    void SetPlayerControl(bool enable)
    {
        if(playerController != null)
        {
            playerController.enabled = enable;
        }
    }
}
