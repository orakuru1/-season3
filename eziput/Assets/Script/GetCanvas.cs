using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCanvas : MonoBehaviour
{
    [Header("ESCキーで開くパネル設定")]
    public GameObject panel;

    [Header("持ち物画面(Imageなど)")]
    public GameObject statusImage;  //全体image
    public GameObject bukiImage;    //武器image
    public GameObject bouguImage;   //防具image
    public GameObject inventoryImage;  //持ち物image

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

        panel.SetActive(newState);
        inventoryImage.SetActive(newState);
        bukiImage.SetActive(newState);
        bouguImage.SetActive(newState);

        if (newState)
        {
            ShowStatusOnly();
            SetPlayerControl(false); // ← 停止！
        }
        else
        {
            SetPlayerControl(true); // ← 再開！
        }
    }

    void HideAllImages()  //最初に全部非表示にするやつ
    {
        if (statusImage) statusImage.SetActive(false);
        if (inventoryImage) inventoryImage.SetActive(false);
        if (bukiImage) bukiImage.SetActive(false);
        if (bouguImage) bouguImage.SetActive(false);
    }

    public void ShowStatusOnly()  //ステータスimageだけ表示
    {
        HideAllImages();
        if (statusImage) statusImage.SetActive(true);
    }

    public void showInventory()  //ステータスimageと持ち物image表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if (inventoryImage) inventoryImage.SetActive(true);
    }

    public void showbuki()  //ステータスimageと武器image表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if (bukiImage) bukiImage.SetActive(true);
    }

    public void showbougu()  //ステータスimageと防具image表示
    {
        HideAllImages();
        if(statusImage) statusImage.SetActive(true);
        if (bouguImage) bouguImage.SetActive(true);
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
