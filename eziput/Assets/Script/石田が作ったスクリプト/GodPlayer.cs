using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodPlayer : MonoBehaviour
{
    //所有している神の力リスト
    public List<GodData> ownedGods = new List<GodData>();
    private const int maxGods = 4;

    //神を追加
    public void AddGod(GodData newGod)
    {
        if (ownedGods.Count >= maxGods)
        {
            Debug.Log("これ以上神を所有できません。");
            return;
        }

        ownedGods.Add(newGod);
        Debug.Log($"{newGod.godName}を所有しました。");
    }

    //神を減らす
    public void RemoveGod(GodData godToRemove)
    {
        if (ownedGods.Remove(godToRemove))
        {
            Debug.Log($"{godToRemove.godName}を所有から外しました。");
        }
        else
        {
            Debug.Log($"{godToRemove.godName}は所有していません。");
        }
    }
    
    //ランダムな神を追加する（デバッグ用）
    public void AddRandomGod()
    {
        GodData randomGod = GodManeger.Instance.GetRandomGod();
        if (randomGod != null)
        {
            AddGod(randomGod);
        }
        else
        {
            Debug.Log("利用可能な神が存在しません。");
        }
    }

    void Start()
    {
        //AddRandomGod();
        //GodManeger.Instance.descriptionGod(ownedGods[0]);
        //GodManeger.Instance.UseGodAbility(ownedGods[0].abilities, this.gameObject, this.gameObject);
        //ownedGods[0].description = "変更された説明文です。";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            GodManeger.Instance.UseGodAbility(ownedGods[0].abilities, this.gameObject, this.gameObject);
        }
    }
}
