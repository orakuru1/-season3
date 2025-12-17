using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodPlayer : MonoBehaviour
{
    //所有している神の力リスト
    public List<GodData> ownedGods = new List<GodData>();
    private Unit u;
    private const int maxGods = 4;

    //神を追加
    public IEnumerator AddGod(GodData newGod)
    {
        //合成できるかのチェック関数
        List<GodData> fusionTargets  = GodManeger.Instance.CheckFusion(ownedGods, newGod);
        if (fusionTargets.Count != 0)
        {
            Debug.Log($"{newGod}と合成できる力があります！");

            yield return StartCoroutine(GodManeger.Instance.FuseGodsCoroutine(fusionTargets, newGod, (fusedGod, RemoveID) =>
            {
                if (RemoveID != -1)
                {
                    GodData removeGod = ownedGods.Find(god => god.id == RemoveID);
                    if (removeGod != null)
                    {
                        ownedGods.Remove(removeGod);
                    }
                    else
                    {
                        Debug.Log("融合する神が見つかりませんでした");
                    }

                }
                ownedGods.Add(fusedGod);///////////////////////////////神の力をゲットしたときに取得する神の力のリセットをしようと思ったら、マイナスになってしまう。
                fusedGod.abilities.ResetCooldown();
            }));
            yield return StartCoroutine(HandleGodLimit());
        }
        else
        {
            ownedGods.Add(newGod);
            newGod.abilities.ResetCooldown();
            Debug.Log($"{newGod.godName}を所有しました。");  
            yield return StartCoroutine(HandleGodLimit());
        }

        //yield return StartCoroutine(HandleGodLimit());
        if (GodUIManager.Instance != null)
        {
            GodUIManager.Instance.UpdateGodIcons(ownedGods);
            GodUIManager ui = FindObjectOfType<GodUIManager>();
            ui.UpdateGodIcons(ownedGods);
        }


    }

    public IEnumerator HandleGodLimit()
    {
        bool isDiscardFinished = false;
        if (ownedGods.Count > maxGods)
        {
            Debug.Log("これ以上神を所有できません。");
            //持ってる力を表示して、選んだ力を捨てる処理をやる↓
            GodDiscardUI1.Instance.Show(ownedGods, (removeid) =>
            {
                if (removeid != -1)
                {
                    GodData removeGod = ownedGods.Find(god => god.id == removeid);
                    if (removeGod != null)
                    {
                        ownedGods.Remove(removeGod);
                    }
                    else
                    {
                        Debug.Log("力を消せませんでした");
                    }
                }
                isDiscardFinished = true;
            });
            yield return new WaitUntil(() => isDiscardFinished);
            yield break;
        }

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

    void Start()
    {
        u = this.GetComponent<Unit>();
        //GodUIManager.Instance.UpdateGodIcons(ownedGods);
        //AddRandomGod();
        //GodManeger.Instance.descriptionGod(ownedGods[0]);
        //GodManeger.Instance.UseGodAbility(ownedGods[0].abilities, this.gameObject, this.gameObject);
        //ownedGods[0].description = "変更された説明文です。";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
