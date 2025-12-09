using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCanvas : MonoBehaviour
{
    [Header("ESCキーで開くパネル設定")]
    public GameObject panel;

    [Header("持ち物画面(Imageなど)")]
    public Transform contentArea;
    public GameObject statusImage;  //全体image
    public GameObject kyarastatusImage; //キャラのステータスimage
    public GameObject bukiImage;    //武器image
    public GameObject bouguImage;   //防具image
    public GameObject inventoryImage;  //持ち物image
    public GameObject setteiImage;  //設定image
    public GameObject itempanel;  //アイテム使いますか？panel
    public GameObject craftArea; 

    [Header("神の力表示Panel")]
    public GameObject kamipanel;

    [Header("プレイヤー操作スクリプト（PlayerMoveなど）")]
    public MonoBehaviour[] playerControllers; // ← 複数登録できるように変更！

    void Start()
    {
        panel.SetActive(false);  //最初はすべて非表示
        HideAllImages();  //最初はimageもすべて非表示
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

    public void TogglePanel()  //ESCキーを押したとき表示非表示する
    {
        bool newState = !panel.activeSelf;
        bool State = panel.activeSelf;

        panel.SetActive(newState);
        kyarastatusImage.SetActive(newState);
        inventoryImage.SetActive(newState);
        bukiImage.SetActive(newState);
        bouguImage.SetActive(newState);
        setteiImage.SetActive(newState);
        itempanel.SetActive(newState);
        kamipanel.SetActive(State);

        if (newState)
        {
            ShowStatusOnly();
            SetPlayerControl(false); // ← 停止！
        }
        else
        {
            SetPlayerControl(true); // ← 再開！

           if(craftmode.instance != null) craftmode.instance.craftArea.gameObject.SetActive(false);
        }
    }

    void HideAllImages()  //最初に全部非表示にするやつ
    {
        if (statusImage) statusImage.SetActive(false);
        if(kyarastatusImage) kyarastatusImage.SetActive(false);
        if (inventoryImage) inventoryImage.SetActive(false);
        if (bukiImage) bukiImage.SetActive(false);
        if (bouguImage) bouguImage.SetActive(false);
        if(setteiImage) setteiImage.SetActive(false);
        if(itempanel) itempanel.SetActive(false);
        if(craftArea) craftArea.SetActive(false);
    }

    public void ShowStatusOnly()  //ステータスimageだけ表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if(kyarastatusImage) kyarastatusImage.SetActive(true);
    }

    public void kyarastatus()
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if(kyarastatusImage) kyarastatusImage.SetActive(true);
    }

    public void showInventory()  //ステータスimageと持ち物image表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if(inventoryImage) inventoryImage.SetActive(true);
        if(craftmode.instance.isCraftMode) 
           craftArea.SetActive(true);
    }

    public void showbuki()  //ステータスimageと武器image表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if (bukiImage) bukiImage.SetActive(true);
        if(craftmode.instance.isCraftMode) craftmode.instance.craftArea.gameObject.SetActive(true);
    }

    public void showbougu()  //ステータスimageと防具image表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if (bouguImage) bouguImage.SetActive(true);
        if(craftmode.instance.isCraftMode) craftmode.instance.craftArea.gameObject.SetActive(true);
    }

    public void settei()  //設定image表示
    {
        HideAllImages();
        if(setteiImage) setteiImage.SetActive(true);
    }

    public void itemsiyou()
    {
        HideAllImages();
        if(itempanel) itempanel.SetActive(true);
    }

    public void CloseInventry()
    {
        ShowStatusOnly();
    }

    public void BGMButton()
    {
        Debug.Log("押されてます");
    }

    void SetPlayerControl(bool enable)
    {
        // 登録されているすべてのプレイヤースクリプトをオン/オフ
        foreach (var script in playerControllers)
        {
            if (script != null)
                script.enabled = enable;
        }

        // マウスカーソルも制御（UI操作しやすく）
        if (!enable)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
