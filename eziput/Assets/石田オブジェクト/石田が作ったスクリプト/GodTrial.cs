using UnityEngine;

[CreateAssetMenu(fileName = "GodTrial", menuName = "God Trial")]
public class GodTrial : ScriptableObject
{
    public string trialName;            // 試練名
    public TrialType trialType;         // 試練の種類


    [Header("基本パラメータ")]
    public int targetValue;             // 目標値（倒す数、ターン数など）
    public int battleID;                // 試練専用バトルID
    public GodData rewardGod;           // 成功報酬（神の力）


    [Header("ステータス条件")]
    public bool requireStatusCheck;     // ステータス条件を使うか
    public StatusType statusType;       // 力/魔力/速さなどの種類
    public int requiredStatusValue;     // 必要ステータス値


    [Header("行動回数条件")]
    public bool requireActionCount;     // ステータス以外の条件
    public ActionType actionType;       // 魔法使用、回復、反撃など
    public int requiredActionValue;


    [Header("位置条件（到達型）")]
    public Vector2Int requiredTilePos;  // 到達マス（任意）
    

    [Header("演出")]
    public GameObject trialStartEffect;
    public GameObject trialSuccessEffect;
}

public enum TrialType
{
    DefeatEnemies,  // 敵を倒す
    SurviveTurns,   // 耐久
    CollectItems,   // アイテム回収
    ReachLocation,  // 場所に到達
    StatusCheck,    // ステータス試練
    ActionCount,    // 行動回数条件（魔法使用数など）
    MiniBattle      // 1vs1バトル
}

public enum StatusType
{
    Strength,
    Magic,
    Dexterity,
    Defense,
    Speed,
    Luck
}

public enum ActionType
{
    UseMagic,
    HealAlly,
    KillEnemy,
    ReceiveDamage,
    BuffUsed
}
