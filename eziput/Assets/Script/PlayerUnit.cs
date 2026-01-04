using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
//using System.Diagnostics;

public class PlayerUnit : Unit
{
    public SkillData selectedSkill = null; // InputHandlerから指定
    public bool isUsingSkill = false; // スキル実行中フラグ       

    public WeaponMasterySet weaponMasterySet = new WeaponMasterySet(); 
    public int durabilityexp = 5;

    private bool leveledUp = false;

    public SkillDatabase allSkillsDatabase; // 全スキルデータベース参照
    
    // スキル使用
    public IEnumerator UseSkill(SkillData skill)
    {
        if (isUsingSkill) yield break; // 二重起動防止
        isUsingSkill = true;
        if (skill == null || skill.attackPattern == null)
        {
            Debug.LogWarning("スキルまたは攻撃パターンが未設定です。");
            yield break;
        }

        animationController.animationState.isAttacking = true;
        Debug.Log($"{status.unitName} が {skill.skillName} を使用しようとしています");

        // 攻撃対象候補を取得
        List<Vector2Int> attackPositions = skill.attackPattern.GetPattern(gridPos, facingDir);
        List<Unit> targets = new List<Unit>();

        foreach (var pos in attackPositions)
        {
            GridBlock targetBlock = gridManager.GetBlock(pos);
            if (targetBlock != null && targetBlock.occupantUnit != null &&
                targetBlock.occupantUnit.team != this.team)
            {
                targets.Add(targetBlock.occupantUnit);
            }
        }

        // 対象がいなければキャンセル
        if (targets.Count == 0)
        {
            Debug.Log("敵が範囲内にいないため、スキルは発動しません。");
            animationController.animationState.isAttacking = false;
            selectedSkill = null;
            isUsingSkill = false; // 終了時に解除
            ClearAttackRange();
            yield break;
        }
/*
        if(skill.weaponType == equippedWeaponType)
        {
            Debug.Log("適切な武器でスキルを使用しました。");
        }
        else
        {
            Debug.Log("警告：適切な武器でスキルを使用していません！");
            animationController.animationState.isAttacking = false;
            selectedSkill = null;
            isUsingSkill = false; // 終了時に解除
            ClearAttackRange();
            yield break;
        }
*/

        Debug.Log($"{status.unitName} が {skill.skillName} を使用！ 対象数: {targets.Count}");

        if (anim != null)
        {
            if (targets.Count == 1)
            {
                animationController.Initialize(targets[0], skill.power, skill.deathAnimationID);
                animationController.AttackAnimation(skill.animationID);
                leveledUp = weaponMasterySet.Get(equippedWeaponType).AddExp(durabilityexp); // 単一対象なら熟練度を5増加
            }
            else
            {
                animationController.Initialize(targets, skill.power, skill.deathAnimationID);
                animationController.AttackAnimation(skill.animationID);
                leveledUp = weaponMasterySet.Get(equippedWeaponType).AddExp(durabilityexp / 2); // 複数対象なら熟練度を半分増加
            }

        }

        if (leveledUp)
        {
            //ここで、新しい技を習得を呼ぶ。
            var Poola = allSkillsDatabase.normalSkills.Get(equippedWeaponType);

            foreach (var ss in Poola)
            {
                if (ss.requiredMastery == weaponMasterySet.Get(equippedWeaponType).level)
                {
                    LearnSkill(ss);
                    //attackSkills.Add(ss);
                }
            }
            Debug.Log($"{status.unitName} の {equippedWeaponType} 熟練度がレベルアップ！ 現在のレベル: {weaponMasterySet.Get(equippedWeaponType).level}");
        }

        ClearAttackRange();

        // ヒットアニメーションが見える時間だけ待機
        yield return new WaitForSeconds(0.3f);

        // プレイヤーの攻撃アニメーション終了を待つ
        if (anim != null)
            yield return new WaitUntil(() => !animationController.animationState.isAttacking);
        else
            yield return null;

        //攻撃の後に神の力をゲットできるか毎回しよう
        yield return StartCoroutine(GodManeger.Instance.GrantGodToPlayer());

        //攻撃する時共通,攻撃時の神の力発動
        yield return StartCoroutine(GodManeger.Instance.TriggerAbilities(this.gameObject, AbilityTrigger.Passive_OnAttack));
        yield return null;

        yield return StartCoroutine(GodManeger.Instance.GrantGodToPlayer());


        // 終了処理
        TurnManager.Instance.NextTurn();
        animationController.animationState.isAttacking = false;
        selectedSkill = null;
        isUsingSkill = false; // 終了時に解除

        // ターン進行は自然に敵が動くように管理


    }

    public void Heal(int amount)
    {
        Debug.Log($"[Heal] {gameObject.name}");
        //Hpが満タンかチェック
        if(status.currentHP >= status.maxHP)
        {
            Debug.Log($"{status.unitName}のHpはすでに満タンです。回復アイテムを使う必要はない！！");
            return;
        }

        int oldHP = status.currentHP;
        status.currentHP = Mathf.Min(status.currentHP + amount, status.maxHP); //maxを超えない

        int healedAmount = status.currentHP - oldHP;  //実際に回復した量
        UpdateHPBar(status.currentHP);
        Debug.Log($"{status.unitName}のHPが{amount}回復！");

    }

    void UpdateHPBar(float currentHP)
    {
        if(hpSlider != null)
        {
            hpSlider.value = (float)status.currentHP / status.maxHP;
        }

        if(hptext != null)
        {
            hptext.text = Mathf.CeilToInt(status.currentHP) + "/" + Mathf.CeilToInt(status.maxHP);
        }
    }

    // 敵ヒット用コルーチン
    private IEnumerator HitTarget(Unit target, int damage)
    {
        // 向き変更（プレイヤーが敵方向を向く）


        // ヒットアニメーション開始（ダメージはまだ）
        if (target.anim != null)
        {
            //target.Target = this;
            target.anim.SetInteger("Hit", 1);
        }

        yield return null; // 即時終了、同時に再生される
    }

    public void UpdateGodUI()
    {
        if(godPlayer != null)
        {
            foreach(var god in godPlayer.ownedGods)
            {
                if(god.abilities != null && god.abilities.floatcurrentCooldown > 0f)
                {
                    if(god.abilities.floatcurrentCooldown < 0f)
                    {
                        god.abilities.floatcurrentCooldown = 0f;
                    }
                }
            }

            if (GodUIManager.Instance != null)
            {
                GodUIManager.Instance.UpdateCooldownUI(godPlayer.ownedGods);
            } 
            
        }
    }
//全部のレベル０とレベル１の技を用意する。
//始まった時に、プレイヤーが何を装備しているから、この技構成を出す。という仕組みを作る。
//武器の装備を変えた時に、技を変えるようにする。（↓の奴を呼ぶ）
    public void RefreshAttackSkills()
    {
        attackSkills.Clear();

        var loadout = weaponSkillLoadouts
            .Find(l => l.weaponType == equippedWeaponType);

        if (loadout == null) return;

        attackSkills.AddRange(loadout.skills);
    }

    public void LearnSkill(SkillData skill)
    {
        var loadout = weaponSkillLoadouts
            .Find(l => l.weaponType == skill.weaponType);

        if (!loadout.skills.Contains(skill))
        {
            loadout.skills.Add(skill);
        }

        // 今その武器を装備していたらUI更新
        if (skill.weaponType == equippedWeaponType)
        {
            RefreshAttackSkills();
        }
    }

    void OnEnable()
    {
        GodManeger.OnGodAbilityUsed += UpdateGodUI;
        GodManeger.CooldownCountAction += UpdateGodUI;
    }

    void OnDisable()
    {
        GodManeger.OnGodAbilityUsed -= UpdateGodUI;
        GodManeger.CooldownCountAction -= UpdateGodUI;
    }

    protected override void Start()
    {
        base.Start(); // ← ここで Unit.Start() が先に実行される

        // Unit の初期化が終わった後にやりたい処理
        UpdateGodUI();
        GodUIManager.Instance.UpdateGodIcons(godPlayer.ownedGods);
    }

    public void Update()
    {

    }

}
