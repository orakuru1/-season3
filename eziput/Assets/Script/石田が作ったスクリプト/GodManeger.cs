using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodManeger : MonoBehaviour
{
    public static GodManeger Instance { get; private set; }
    public List<GodData> allgods = new List<GodData>();

//クールダウンに入った時に送り込む辞書
    private Dictionary<GodAbility, float> cooldownTimers = new Dictionary<GodAbility, float>();

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        LoadAllGods();
    }

    //最初にリソースフォルダの神データを全部読み込む
    private void LoadAllGods()
    {
        allgods.Clear();
        GodData[] loadedGods = Resources.LoadAll<GodData>("Gods");
        allgods.AddRange(loadedGods);
        Debug.Log($"Loaded {allgods.Count} gods.");
    }

    //ランダムで神を取得    
    public GodData GetRandomGod()
    {
        if (allgods.Count == 0) return null;
        return allgods[Random.Range(0, allgods.Count)];
    }

    /// 神の詳細を表示するデバッグ用メソッド
    public void descriptionGod(GodData god)
    {
        Debug.Log($"神の名前: {god.godName}\n肩書き: {god.title}\n能力: {god.abilities.abilityName}\n説明: {god.description}");
    }

    //神の力を発動させるメソッド
    public void UseGodAbility(GodAbility ability, GameObject user, GameObject target)
    {
        if (cooldownTimers.ContainsKey(ability))
        {
            Debug.Log($"{ability.abilityName} はクールダウン中です。残り時間: {cooldownTimers[ability]:F2} 秒");
            return;
        }

        switch (ability.type)
        {
            case AbilityType.Attack:
                Debug.Log($"{ability.abilityName} を発動！（攻撃力 {ability.power}）");
                cooldownTimers[ability] = ability.cooldown;
                ability.isActive = true;
                target.GetComponent<Unit>()?.TakeDamage(ability.power);
                // ここで敵にダメージを与える処理を書く
                break;

            case AbilityType.Heal:
                Debug.Log($"{ability.abilityName} を発動！（回復量 {ability.power}）");
                cooldownTimers[ability] = ability.cooldown;
                ability.isActive = true;
                //user.GetComponent<Unit>()?.Heal(ability.power);まだない
                // HPを回復する処理
                break;

            case AbilityType.Buff:
                Debug.Log($"{ability.abilityName} の加護で強化！({ability.description})");
                cooldownTimers[ability] = ability.cooldown;
                ability.isActive = true;
                //user.GetComponent<Unit>()?.Buff(ability.power);まだない
                // ステータス強化処理
                break;

            case AbilityType.Debuff:
                Debug.Log($"{ability.abilityName} の呪いを発動！({ability.description})");
                cooldownTimers[ability] = ability.cooldown;
                ability.isActive = true;
                //target.GetComponent<Unit>()?.Debuff(ability.power);まだない
                // 相手の弱体化処理
                break;

            case AbilityType.Special:
                Debug.Log($"{ability.abilityName} の特別な力を行使！");
                cooldownTimers[ability] = ability.cooldown;
                ability.isActive = true;
                //target.GetComponent<Unit>()?.Special(ability.power);まだない
                // 特殊効果
                break;
        }
        //Debug.Log($"{user.name} が {target.name} に {ability.abilityName} を使った！");
    }
    /// <summary>
    /// 呼び出し例
    /// <ターン開始時>
    /// GodManeger.TriggerAbilities(currentUnit.gameObject, AbilityTrigger.Passive_OnTurnStart);
    /// <ターン終了時>
    /// GodManeger.TriggerAbilities(attacker.gameObject, AbilityTrigger.Passive_OnAttack, defender.gameObject);
    /// </summary>


    //ここで、ターン開始時、ターン終了時、攻撃時、被弾時などのタイミングで呼び出す。パッシブ系を呼ぶところ
    public void TriggerAbilities(GameObject unit, AbilityTrigger trigger, GameObject target = null)//この書き方は？=null
    {
        var godPlayer = unit.GetComponent<GodPlayer>();
        if (godPlayer == null) return;

        foreach (var god in godPlayer.ownedGods)
        {

            var ability = god.abilities;
            
            if (ability.isActive != false) continue;

            // 対応するトリガーか判定
            if (ability.trigger != trigger) continue;

            Debug.Log($"{unit.name} の {god.godName} の {ability.abilityName} が {trigger} で発動！");
            switch(ability.type)
            {
                case AbilityType.Attack:
                    UseGodAbility(ability, unit, target);
                    break;
                case AbilityType.Heal:
                    UseGodAbility(ability, unit, target);
                    break;
                case AbilityType.Buff:
                    UseGodAbility(ability, unit, target);
                    break;
                case AbilityType.Debuff:
                    UseGodAbility(ability, unit, target);
                    break;
                // 他のタイプもここで処理可能
            }

        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //クールダウンを減らす処理を今は時間でやってるから、ターンが経過ごとに減らすようにする
        List<GodAbility> keys = new List<GodAbility>(cooldownTimers.Keys);
        foreach (var ability in keys)
        {
            cooldownTimers[ability] -= Time.deltaTime;
            if (cooldownTimers[ability] <= 0)
            {
                cooldownTimers.Remove(ability);
                Debug.Log($"{ability.abilityName} のクールダウンが終了しました。");
                ability.isActive = false;
            }
        }
    }
}
