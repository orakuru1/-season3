using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{
    private Unit unit;
    private Slider BossSliderInstance;

    [Header("ボスドロップ")]
    [SerializeField] private string dropItemName;
    [SerializeField] private ItemType dropItemType;
    [SerializeField] private float dropDelay = 0.3f;

    public enum ItemType{Item, Weapon, Armor}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OnBossSliderTrue();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 逃げたら消す（好み）
        OnBossSliderFalse();
    }

    public void OnBossSliderTrue()
    {
        BossSliderInstance.gameObject.SetActive(true);
    }
    public void OnBossSliderFalse()
    {
        BossSliderInstance.gameObject.SetActive(false);
    }

    private void Start()
    {
        unit = GetComponent<Unit>();
        
        BossSliderInstance = BossUIManeger.Instance.Initialize(GetComponent<Unit>());
        unit.hpSlider = BossSliderInstance;
        Debug.Log($"ドロップアイテム:{dropItemName}");
    }

    public void enemyDie()
    {
        StartCoroutine(DropItemCoroutine());
        OnBossSliderFalse();
        Debug.Log("敵が死んだ");
    }

    private IEnumerator DropItemCoroutine()
    {
        //死亡演出待ち
        yield return new WaitForSeconds(dropDelay);

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
