using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodTrialManeger : MonoBehaviour
{
    public static GodTrialManeger Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }
    
    //神の試練が発生したときに呼ばれる関数
    public void TriggerGodTrial(GodTrial trialGods, Unit unit)
    {
        AssignTrialGods(trialGods, unit);
    }

    //神の試練の種類を見て、割り振る関数
    public void AssignTrialGods(GodTrial trialGods, Unit unit)
    {
        bool isCleared = trialGods.trialType switch
        {
            TrialType.DefeatEnemies => false,
            TrialType.SurviveTurns => false,
            TrialType.CollectItems => false,
            TrialType.ReachLocation => false,
            TrialType.StatusCheck => CheckStatusTrial(trialGods, unit),
            TrialType.ActionCount => CheckActionCountTrial(trialGods, unit),
            TrialType.MiniBattle => false,
            _ => false,
        };

        if(isCleared)
        {
            //試練クリアの処理
            GodManeger.Instance.addinggods(trialGods.rewardGod, unit.GetComponent<GodPlayer>());//追加のリストに入っただけだから、まだ追加されていない

        }
        else
        {
            //試練失敗の処理
            Debug.Log("試練失敗");
        }
    }

    //今までの使用回数の試練だったら、クリアしているかを確認する関数
    //まだ、何回使ったかを数える処理を作っていない。
    public bool CheckActionCountTrial(GodTrial trialgods, Unit unit)
    {
        int actionCount = trialgods.actionType switch
        {
            ActionType.UseMagic => 0,
            ActionType.HealAlly => 0,
            ActionType.KillEnemy => 0,
            ActionType.ReceiveDamage => 0,
            ActionType.BuffUsed => 0,
            _ => 0,
        };
        return actionCount >= trialgods.requiredActionValue;
    }

    //ステータスの試練だったら、クリアしているかを確認する関数
    public bool CheckStatusTrial(GodTrial trialGods, Unit unit)
    {
        int statusValue = trialGods.statusType switch
        {
            StatusType.Strength => unit.status.attack,
            //StatusType.Magic => unit.status.magic,
            //StatusType.Dexterity => unit.status.dexterity,
            StatusType.Defense => unit.status.defense,
            StatusType.Speed => unit.status.speed,
            StatusType.Luck => unit.status.luck,
            _ => 0,
        };

        return statusValue >= trialGods.requiredStatusValue;
    }
}

