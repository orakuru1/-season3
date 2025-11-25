using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class GodManeger : MonoBehaviour
{
    public static GodManeger Instance { get; private set; }
    public List<GodData> allgods = new List<GodData>();
    private List<GodData> addgods = new List<GodData>();

    //クールダウンに入った時に送り込む辞書
    private Dictionary<GodAbility, float> cooldownTimers = new Dictionary<GodAbility, float>();

    private int ForCount = 0;

    public bool isGodDescrip = false;
    private bool isDed = false;

    public List<Vector3> enemyPosition = new List<Vector3>();

    public List<FusionRecipe> fusionRecipes = new List<FusionRecipe>();
    
    private GodPlayer GetPlayer;

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
#region 神の力を追加する関係
    public void addinggods(List<GodData> god, GodPlayer getplayer, bool isded)
    {
        if (god == null || getplayer == null)  return;

        addgods.AddRange(god);
        GetPlayer = getplayer;
        isDed = isded;
    }
    public IEnumerator GrantGodToPlayer()
    {
        if(addgods.Count == 0 || GetPlayer == null) yield break;

        isGodDescrip = true;
        foreach(var god in addgods)
        {
            yield return StartCoroutine(GetPlayer.AddGod(god));
            Debug.Log($"{GetPlayer.name} は {god.godName} の加護を授かった！");  
        }
        addgods.Clear();
        GetPlayer = null;
        isDed = false;
        isGodDescrip = false;
    }
#endregion

    //// <summary>
    /// 呼び出し例
    /// <ターン開始時>
    /// GodManeger.TriggerAbilities(currentUnit.gameObject, AbilityTrigger.Passive_OnTurnStart);
    /// <ターン終了時>
    /// GodManeger.TriggerAbilities(attacker.gameObject, AbilityTrigger.Passive_OnAttack, defender.gameObject);
    /// </summary>

    //ここで、ターン開始時、ターン終了時、攻撃時、被弾時などのタイミングで呼び出す。パッシブ系を呼ぶところ
#region 神の力のチェック
    public IEnumerator TriggerAbilities(GameObject unit, AbilityTrigger trigger)
    {
        yield return null;
        ForCount = 0;//初期化
        var godPlayer = unit.GetComponent<GodPlayer>();
        Unit u = unit.GetComponent<Unit>();
        AnimationController animationController = unit.GetComponent<AnimationController>();
        if (godPlayer == null)
        {
            Debug.Log("神の力をもっていないよ");
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
                    yield return StartCoroutine(ActivateGodAttack(unit.GetComponent<Unit>(), god));
                    break;
                case AbilityType.Heal://ターゲットいらない
                    yield return StartCoroutine(HealAbility(ability, unit));
                    break;
                case AbilityType.Buff://ターゲットいらない
                    yield return StartCoroutine(BuffAbility(ability, unit));
                    break;
                case AbilityType.Debuff:
                    yield return StartCoroutine(ActivateGodAttack(unit.GetComponent<Unit>(), god));
                    break;
                    // 他のタイプもここで処理可能
            }
        }

        Debug.Log("神の力の発動処理終了");

    }
#endregion

#region 効果発動
    public void AttackAbility(GodAbility ability, GameObject user, Unit target)
    {
        Debug.Log($"{target}に{ability.abilityName} を発動！（攻撃力 {ability.power}）");
        //target.TakeDamage(ability.power, user.GetComponent<Unit>());//animatorの結びつけが大変そう、攻撃アニメーションだから。
        ability.isActive = true;
    }

    public void DebuffAbility(GodAbility ability, GameObject user, Unit target)
    {
        Debug.Log($"{target}に{ability.abilityName} を発動！（攻撃力 {ability.power}）");
        //target.TakeDebuff(ability.power, user.GetComponent<Unit>());
        ability.isActive = true;
    }

    public IEnumerator HealAbility(GodAbility ability, GameObject user)
    {
        yield return null;
        if (cooldownTimers.ContainsKey(ability))
        {
            Debug.Log($"{ability.abilityName} はクールダウン中です。残り時間: {cooldownTimers[ability]:F2} 秒");
            yield break;
        }

        ForCount++;
        Debug.Log($"{ability.abilityName} を発動！（回復量 {ability.power}）");
        cooldownTimers[ability] = ability.cooldown;///////////////////////クールダウンがない場合は、入れないにしよう。
        ability.isActive = true;

        //ヒールのアニメーション
        yield return null;
        AnimationController animationController = user.GetComponent<AnimationController>();
        animationController.InitializeHill(user.GetComponent<Unit>(), ability.power);
        animationController.HillAnimation(ability.GodAnimationID);

        cooldownTimers[ability] = ability.cooldown;
        yield return new WaitUntil(() => !animationController.animationState.isHiling);
        Debug.Log("ヒールアニメーション終了");
    }

    public IEnumerator BuffAbility(GodAbility ability, GameObject user)
    {
        yield return null;
        if (cooldownTimers.ContainsKey(ability))
        {
            Debug.Log($"{ability.abilityName} はクールダウン中です。残り時間: {cooldownTimers[ability]:F2} 秒");
            yield break;
        }
        
        ForCount++;
        Debug.Log($"{ability.abilityName} の加護で強化！({ability.description})");
        cooldownTimers[ability] = ability.cooldown;///////////////////////クールダウンがない場合は、入れないにしよう。
        ability.isActive = true;

        //バフのアニメーション
        yield return null;
        AnimationController animationController = user.GetComponent<AnimationController>();
        animationController.InitializeBuff(user.GetComponent<Unit>(), ability.power);
        animationController.BuffAnimation(ability.GodAnimationID);

        cooldownTimers[ability] = ability.cooldown;
        yield return new WaitUntil(() => !animationController.animationState.isBuffing);
        Debug.Log("バフアニメーション終了");

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
            enemyPosition.Clear();
            foreach(var target in targets)
            {
                enemyPosition.Add(target.transform.position);
            }
            ForCount++;
            yield return null;//相手がアニメーターを持っていないと早すぎるので、待つ
            switch (ability.type)
            {
                case AbilityType.Attack:
                        /////////////////////////実際の攻撃処理はこんな感じ
                        Debug.Log("範囲内に敵がいるのでゴッドアニメーション発動！");
                        AnimationController animationController = user.GetComponent<AnimationController>();//ここら辺の処理は、attack等にそれぞれでまとめる。
                        animationController.Initialize(targets, ability.power);
                        animationController.AttackAnimation(ability.GodAnimationID);//いずれ、神のアニメーションとアニメーション番号を作る（最初１，１個習合２，みたいな感じ）
                        ///////////////////////////////////////////////////
                        cooldownTimers[ability] = ability.cooldown;///////////////////////クールダウンがない場合は、入れないにしよう。（関数で共通化）SetCooldown(ability);
                        yield return new WaitUntil(() => !animationController.animationState.isAttacking);
                    break;
                case AbilityType.Debuff:
                        //デバフのアニメーション
                        AnimationController animationController2 = user.GetComponent<AnimationController>();
                        animationController2.InitializeDebuff(user.GetComponent<Unit>(), ability.power);
                        animationController2.DebuffAnimation(ability.GodAnimationID);

                        cooldownTimers[ability] = ability.cooldown;
                        yield return new WaitUntil(() => !animationController2.animationState.isDebuffing);
                        Debug.Log("デバフアニメーション終了");
                    break;
            }

        }
    }
#endregion




    /// <summary>
    /// 範囲パターンに基づいて対象ユニットを取得
    /// </summary>
#region 力の範囲に敵がいるか
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

                case GodAttackPattern.Forward1:
                    // 前方1マス（向いている方向）
                    if (dir == Vector2Int.up && dy == 1 && dx == 0)
                        targets.Add(unit);
                    if (dir == Vector2Int.down && dy == -1 && dx == 0)
                        targets.Add(unit);
                    if (dir == Vector2Int.left && dx == -1 && dy == 0)
                        targets.Add(unit);
                    if (dir == Vector2Int.right && dx == 1 && dy == 0)
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
#endregion


#region 力の融合処理
    public IEnumerator FuseGodsCoroutine(List<GodData> fusionCandidates, GodData newGod, System.Action<GodData, int> onFinish)
    {
        bool fusionFinished  = false;

        FusionUI.Instance.Show(fusionCandidates, newGod, (isYES, RemoveID, sentakuGod) =>
        {
            if (isYES)
            {
                GodData fusiongod = CheckFusionGod(sentakuGod, newGod);
                if(fusiongod == null)
                {
                    Debug.Log("融合できるはずなのにできませんでした。");
                    onFinish?.Invoke(newGod, RemoveID);
                    fusionFinished = true;
                    return;
                }
                onFinish?.Invoke(fusiongod, RemoveID);
                fusionFinished = true;
                //どうやって融合された神の力を出そうか？幅を持たせたいからなんかの計算式になりそう。それか、力技でデータに全部入れ込む
                //全部の神をこのスクリプトが持ってるから、ここを呼ぶのがいいのかな？融合後の力が欲しいなら。
            }
            else
            {
                //NOが選ばれたらnewGodをそのまま追加
                onFinish?.Invoke(newGod, RemoveID);
                fusionFinished  = true;
            }
        });

        // UI操作が終わるまで待つ
        yield return new WaitUntil(() => fusionFinished);
        yield break;
    }

//融合後の神の力を渡す
    public GodData CheckFusionGod(GodData gods, GodData newGod)
    {
        foreach(var recipe in fusionRecipes)
        {
            if((recipe.god1 == gods && recipe.god2 == newGod) || (recipe.god1 == newGod && recipe.god2 == gods))
            {
                return recipe.resultGod;
            }
        }
        return null;
    }
//融合できる神のリストを渡す
    public List<GodData> CheckFusion(List<GodData> ownegods, GodData newGod)
    {
        List<GodData> fusionTargets = new List<GodData>();
        foreach(var ownedGod in ownegods)
        {
            foreach(var recipe in fusionRecipes)
            {
                if((recipe.god1 == ownedGod && recipe.god2 == newGod) || (recipe.god1 == newGod && recipe.god2 == ownedGod))
                {
                    fusionTargets.Add(ownedGod);
                }
            }
        }
        return fusionTargets;
    }
#endregion


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
