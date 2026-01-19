using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WallHint : MonoBehaviour
{
    Renderer[] renderers;

    public float interactRange = 1.5f;
    public string ItemName; //獲得できるアイテム名

    bool obtained = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false); // 初期は非表示
    }

    public void SetVisible(bool visible)
    {
        foreach (var r in renderers)
            r.enabled = visible;
    }

    public bool CanInteract(Transform player)
    {
        if (obtained) return false;

        float dist = Vector3.Distance(
            player.position,
            transform.position
        );

        return dist <= interactRange;
    }

    public void Interact()
    {
        if (obtained) return;

        obtained = true;

        // アイテム獲得
        //ItemManager.Instance.AddItem(itemId);
        StartCoroutine(DropItemCoroutine(ItemName));

        // 演出（消える）
        WallHintManager.Instance.RemoveHint(this);
        Destroy(gameObject);
    }

    private IEnumerator DropItemCoroutine(string dropItemName)
    {
        //死亡演出待ち
        //yield return new WaitForSeconds(0.2f);

        if(ItemUIManager.instance == null)
        {
            Debug.Log("ItemUIManagerが存在しません");
            yield break;
        }
        //アイコン取得
        Sprite icon = null;
        if(ItemUIManager.instance.itemDataDict.ContainsKey(dropItemName))
        {
            icon = ItemUIManager.instance.itemDataDict[dropItemName].icon;
        }

        //取得UI表示
        if(GetItemPopUi.instance != null)
        {
            GetItemPopUi.instance.Show(dropItemName, icon);
        }

        //カテゴリ取得
        string category = ItemUIManager.instance.GetItemCategory(dropItemName);
        if(category == null)
        {
            Debug.Log($"カテゴリ不明のアイテム: {dropItemName}");
            yield break;
        }
        //追加
        ItemUIManager.instance.AddItem(dropItemName, category);

        Destroy(gameObject);

        Debug.Log($"ボス撃破ドロップ：{dropItemName}");
    }
}
