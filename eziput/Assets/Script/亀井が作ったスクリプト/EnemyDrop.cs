using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    /*[Header("雑魚敵ドロップ設定")]
    public bool hasDrop = true;
    public List<string> dropItemNames = new List<string>();
    public List<animation.ItemType> dropItemTypes = new List<animation.ItemType>();

    [Range(0f, 1f)]
    public float dropChance = 1.0f;  //雑魚は確率制

    public enum ItemType{Item, Weapon, Armor}

    public void Drop()
    {
        if(!hasDrop) return;
        if(dropItemNames.Count == 0) return;

        if(Random.value > dropChance)
        {
            LogManager.Instance.AddLog($"{LogManager.ColorText("アイテムはドロップしなかった", "#ffffff")}");
            return;
        }

        int index = Random.Range(0, dropItemNames.Count);

        string name = dropItemNames[index];
        animation.ItemType type = dropItemTypes[index];

        LogManager.Instance.AddLog($"敵が{LogManager.ColorText("アイテム", "#FF4444")}をドロップした");

        switch(type)
        {
            case ItemType.Item:
                ItemUIManager.instance.AddItem(name);
                break;

            case ItemType.Weapon:
                ItemUIManager.instance.AddWeapon(name);
                break;

            case ItemType.Armor:
                ItemUIManager.instance.AddArmor(name);
                break;
        }
    }*/
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
