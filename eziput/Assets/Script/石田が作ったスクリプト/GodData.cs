using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 神の基本データ
/// </summary>
[CreateAssetMenu(fileName = "NewGodData", menuName = "Mythology/God Data")]
public class GodData : ScriptableObject
{
    public string godName;         // 神の名前
    public string title;           // 神の肩書き（例：雷神・知恵の神）
    public string mythOrigin;      // 登場神話（例：ギリシャ、エジプトなど）
    public string description;     // 説明文
    public Sprite icon;            // UI表示用アイコン
    public GodAbility abilities; // 神が持つ力
}

[System.Serializable]
public class GodAbility
{
    public string abilityName;     // 技・効果名
    public string description;     // 説明
    public int power;              // 効果の強さ
    public AbilityTrigger trigger; // 発動タイプ
    public AbilityType type;       // 力の種類（攻撃・回復・強化など）
    public GodAttackPattern attackPattern; // 攻撃パターン（攻撃タイプの場合）

    [Header("発動管理")]
    public float cooldown;         // クールダウン時間
    [HideInInspector] public float floatcurrentCooldown; // 現在のクールダウン時間
    [HideInInspector] public bool isActive;        // 発動中かどうか
    public int GodAnimationID;//普通の数字はプレイヤーの攻撃で使ってるけど、神の攻撃は1000番台を使おう
}

public enum AbilityTrigger
{
    Passive_OnStart,      // 戦闘開始時に発動
    Passive_OnTurnStart,   // 自分のターン開始時
    Passive_OnTurnEnd,    // ターン終了時に発動
    Passive_OnAttack,     // 攻撃した時に発動
    Passive_OnDamaged,    // 被弾時に発動
    Active_OnUse          // プレイヤーが使用した時（今のアクティブ）
}

public enum AbilityType
{
    Attack,
    Heal,
    Buff,
    Debuff,
    Special
}

public enum GodAttackPattern
{
    None,
    Surround8,  // 自分の周囲8マス
    Forward3,   // 前方3マス
    Cross5,     // 上下左右＋中央
    Global      // 敵全体
}


