using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class GodManeger : MonoBehaviour
{
    public static GodManeger Instance { get; private set; }
    public List<GodData> allgods = new List<GodData>();

    //クールダウンに入った時に送り込む辞書
    private Dictionary<GodAbility, float> cooldownTimers = new Dictionary<GodAbility, float>();

    public bool isActiveGod;

    private int ForCount = 0;

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

        foreach(var god in allgods)//いずれは、セーブして戻った時に、trueにしてないといけない時もある。でも、新しいステージに行ったとき等にfalseにするかも。それか、全部読み込んだあとに、ロードするか。
        {
            if (god.abilities != null) god.abilities.isActive = false;
        }
    }
/*
    //ランダムで神を取得    
    public GodData GetRandomGod()
    {
        if (allgods.Count == 0) return null;
        return allgods[Random.Range(0, allgods.Count)];
    }
*/
    public IEnumerator GrantGodToPlayer(GodData god, GodPlayer player)
    {
        if (god == null || player == null) yield break;

        yield return StartCoroutine(player.AddGod(god));
        Debug.Log($"{player.name} は {god.godName} の加護を授かった！");
    }

    /// 神の詳細を表示するデバッグ用メソッド
    public void descriptionGod(GodData god)
    {
        Debug.Log($"神の名前: {god.godName}\n肩書き: {god.title}\n能力: {god.abilities.abilityName}\n説明: {god.description}");
    }

    //// <summary>
    /// 呼び出し例
    /// <ターン開始時>
    /// GodManeger.TriggerAbilities(currentUnit.gameObject, AbilityTrigger.Passive_OnTurnStart);
    /// <ターン終了時>
    /// GodManeger.TriggerAbilities(attacker.gameObject, AbilityTrigger.Passive_OnAttack, defender.gameObject);
    /// </summary>

    //ここで、ターン開始時、ターン終了時、攻撃時、被弾時などのタイミングで呼び出す。パッシブ系を呼ぶところ
    public IEnumerator TriggerAbilities(GameObject unit, AbilityTrigger trigger)
    {
        yield return null;
        ForCount = 0;//初期化
        isActiveGod = true;
        var godPlayer = unit.GetComponent<GodPlayer>();
        Unit u = unit.GetComponent<Unit>();
        AnimationController animationController = unit.GetComponent<AnimationController>();
        if (godPlayer == null)
        {
            if (u.team == Unit.Team.Player)
            {
                animationController.animationState.isAttacking = false;
                TurnManager.Instance.NextTurn();
            }
            Debug.Log("神の力をもっていないよ");
            isActiveGod = false;
            yield break;
        }

        var CopyGodPlayer = godPlayer.ownedGods.ToList();
        foreach (var god in CopyGodPlayer)
        {

            var ability = god.abilities;

            if (ability.isActive != false) continue;

            // 対応するトリガーか判定
            if (ability.trigger != trigger) continue;

            Debug.Log($"{unit.name} の {god.godName} の {ability.abilityName} が {trigger} で発動！");
            switch (ability.type)
            {
                case AbilityType.Attack:///////////////全ての攻撃やヒール等の専用関数を作って、UseGodAbilityは消す。ActivateGodAttackが必要なのは、その先で、攻撃とデバフのどっちに行くかを判断する。
                    Debug.Log("範囲攻撃タイプの神の力発動");
                    yield return StartCoroutine(ActivateGodAttack(unit.GetComponent<Unit>(), god));
                    break;
                case AbilityType.Heal://ターゲットいらない
                    yield return StartCoroutine(HealAbility(ability, unit));
                    break;
                case AbilityType.Buff://ターゲットいらない
                    yield return StartCoroutine(BuffAbility(ability, unit));
                    break;
                case AbilityType.Debuff:
                    Debug.Log("範囲攻撃タイプの神の力発動");
                    yield return StartCoroutine(ActivateGodAttack(unit.GetComponent<Unit>(), god));
                    break;
                    // 他のタイプもここで処理可能
            }
        }

        if (ForCount == 0)//一回でもあったら、アニメーションを通すはずなので、何もないなら次に進む。
        {
            
        }
        if (u.team == Unit.Team.Player)
        {
            animationController.animationState.isAttacking = false;
            TurnManager.Instance.NextTurn();//ここに置いていると、敵を神の力で倒したときに、もう一回行動できる。
            //外に置くと、もう一回行動は無くなるが、敵が無限に行動する。
        }
        isActiveGod = false;
        //TurnManager.Instance.NextTurn();

    }

    public void AttackAbility(GodAbility ability, GameObject user, Unit target)
    {
        Debug.Log($"{target}に{ability.abilityName} を発動！（攻撃力 {ability.power}）");
        //target.TakeDamage(ability.power, user.GetComponent<Unit>());//animatorの結びつけが大変そう、攻撃アニメーションだから。
        ability.isActive = true;

    }

    public IEnumerator HealAbility(GodAbility ability, GameObject user)
    {
        if (cooldownTimers.ContainsKey(ability))
        {
            Debug.Log($"{ability.abilityName} はクールダウン中です。残り時間: {cooldownTimers[ability]:F2} 秒");
            yield break;
        }

        ForCount++;
        Debug.Log($"{ability.abilityName} を発動！（回復量 {ability.power}）");
        cooldownTimers[ability] = ability.cooldown;///////////////////////クールダウンがない場合は、入れないにしよう。
        ability.isActive = true;

        /////////////////////////ヒールのアニメーションを入れる//////////////////////////////////////////////
    }

    public IEnumerator BuffAbility(GodAbility ability, GameObject user)
    {
        if (cooldownTimers.ContainsKey(ability))
        {
            Debug.Log($"{ability.abilityName} はクールダウン中です。残り時間: {cooldownTimers[ability]:F2} 秒");
            yield break;
        }
        
        ForCount++;
        Debug.Log($"{ability.abilityName} の加護で強化！({ability.description})");
        cooldownTimers[ability] = ability.cooldown;///////////////////////クールダウンがない場合は、入れないにしよう。
        ability.isActive = true;
        /////////////////////////バフのアニメーションを入れる//////////////////////////////////////////////
    }
    
    public void DebuffAbility(GodAbility ability, GameObject user, Unit target)
    {

        Debug.Log($"{target}に{ability.abilityName} を発動！（攻撃力 {ability.power}）");
        //target.TakeDebuff(ability.power, user.GetComponent<Unit>());
        ability.isActive = true;
    }

    /// <summary>
    /// 神の力を発動（範囲攻撃タイプ）
    /// </summary>
    public IEnumerator ActivateGodAttack(Unit user, GodData god)
    {
        var ability = god.abilities;

        if (cooldownTimers.ContainsKey(ability))
        {
            Debug.Log($"{ability.abilityName} はクールダウン中です。残り時間: {cooldownTimers[ability]:F2} 秒");
            yield break;
        }

        if (god.abilities.attackPattern == GodAttackPattern.None) yield break;
        List<Unit> targets = GetTargetsInRange(user, god.abilities.attackPattern).ToList();

        Debug.Log($"{user.name} の神『{god.godName}』が発動！ 対象数:{targets.Count}");

        foreach (var target in targets)
        {

            switch (ability.type)
            {
                case AbilityType.Attack:
                    Debug.Log($"{target.name} に {god.abilities.abilityName} の効果を適用！");
                    AttackAbility(god.abilities, user.gameObject, target);
                    break;
                case AbilityType.Debuff:
                    Debug.Log($"{target.name} に {god.abilities.abilityName} の効果を適用！");
                    DebuffAbility(god.abilities, user.gameObject, target);
                    break;
                    // 他のタイプもここで処理可能
            }

            //int damage = god.power;
            //target.TakeDamage(damage, user);
        }

        if(targets.Count != 0)
        {
            ForCount++;
            /////////////////////////実際の攻撃処理はこんな感じ
            Debug.Log("範囲内に敵がいるのでゴッドアニメーション発動！");
            AnimationController animationController = user.GetComponent<AnimationController>();//ここら辺の処理は、attack等にそれぞれでまとめる。
            animationController.Initialize(targets, ability.power);
            animationController.AttackAnimation(ability.GodAnimationID);//いずれ、神のアニメーションとアニメーション番号を作る（最初１，１個習合２，みたいな感じ）
            ///////////////////////////////////////////////////
            cooldownTimers[ability] = ability.cooldown;///////////////////////クールダウンがない場合は、入れないにしよう。（関数で共通化）SetCooldown(ability);
            yield return new WaitUntil(() => !animationController.animationState.isAttacking);
        }


    }

    /// <summary>
    /// 範囲パターンに基づいて対象ユニットを取得
    /// </summary>
    private List<Unit> GetTargetsInRange(Unit user, GodAttackPattern pattern)
    {
        List<Unit> targets = new List<Unit>();
        Vector2Int origin = user.gridPos;

        // 敵チームだけ対象にする
        List<Unit> allunits = TurnManager.Instance.GetAllUnits();
        foreach (var unit in allunits)
        {
            if (unit == user) continue;
            if (unit.team == user.team) continue;

            Vector2Int pos = unit.gridPos;
            Vector2Int dir = user.facingDir;
            int dx = pos.x - origin.x;
            int dy = pos.y - origin.y;

            switch (pattern)
            {
                case GodAttackPattern.Surround8:
                    if (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1)
                        targets.Add(unit);
                    break;

                case GodAttackPattern.Forward3:
                    // 前方3マス（向いている方向）
                    if (dir == Vector2Int.up && dy > 0 && dy <= 3 && dx == 0)
                        targets.Add(unit);
                    if (dir == Vector2Int.down && dy < 0 && dy >= -3 && dx == 0)
                        targets.Add(unit);
                    if (dir == Vector2Int.left && dx < 0 && dx >= -3 && dy == 0)
                        targets.Add(unit);
                    if (dir == Vector2Int.right && dx > 0 && dx <= 3 && dy == 0)
                        targets.Add(unit);
                    break;

                case GodAttackPattern.Cross5:
                    // 上下左右+中央（十字範囲）
                    if ((Mathf.Abs(dx) <= 2 && dy == 0) || (Mathf.Abs(dy) <= 2 && dx == 0))
                        targets.Add(unit);
                    break;

                case GodAttackPattern.Global:
                    // 全ての敵
                    targets.Add(unit);
                    break;
            }
        }

        return targets;
    }


        //新しい力と融合できる神の力UIを表示
        //「習合するボタン」と「しない」ボタンを表示
        //習合したい力を選択した状態で「習合する」ボタンを押すと習合開始
        //１つの力になる
        //神の力リストに入る


    public IEnumerator FuseGodsCoroutine(List<GodData> fusionCandidates, GodData newGod, System.Action<GodData, int> onFinish)
    {
        bool fusionFinished  = false;

        FusionUI.Instance.Show(fusionCandidates, newGod, (isYES, RemoveID) =>
        {
            if (isYES)
            {
                onFinish?.Invoke(newGod.fusionGod, RemoveID);
                fusionFinished = true;
                //どうやって融合された神の力を出そうか？幅を持たせたいからなんかの計算式になりそう。それか、力技でデータに全部入れ込む
                //全部の神をこのスクリプトが持ってるから、ここを呼ぶのがいいのかな？融合後の力が欲しいなら。
            }
            else
            {//NOが選ばれた場合神の力の上限を超えるかもしれない
                onFinish?.Invoke(newGod, RemoveID);
                fusionFinished  = true;
            }
        });

        // UI操作が終わるまで待つ
        yield return new WaitUntil(() => fusionFinished);
        yield break;
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
