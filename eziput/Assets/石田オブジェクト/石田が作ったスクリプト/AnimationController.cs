using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Unit Im;

    public List<Unit> Target = new List<Unit>(); // 攻撃する先
    public int damage;
    private int remaininghits;
    public AnimationController attacker; // 攻撃してきた人
    public AnimationState animationState = new AnimationState();//条件を構造体にまとめた

    private Animator anim;


    public System.Action onAnimationEnd;// アニメーション終了時に呼ばれるイベント

    void Start()
    {
        Im = GetComponent<Unit>();
        anim = GetComponent<Animator>();
    }

    public void Initialize(Unit target, int dmg = 0)
    {
        Target.Clear();
        Target.Add(target);
        damage = dmg;
        animationState.isAttacking = true;
        animationState.ismultipleTaget = false;
    }
    public void Initialize(List<Unit> targets, int dmg = 0)
    {
        Target.Clear();
        Target = targets;
        damage = dmg;
        animationState.isAttacking = true;
        animationState.ismultipleTaget = true;
    }

    public void InitializeBuff(Unit user, int power = 0)
    {
        Target.Clear();
        Target.Add(user);
        damage = power;

        animationState.isBuffing = true;
    }

    public void InitializeDebuff(Unit target, int power = 0)
    {
        Target.Clear();
        Target.Add(target);
        damage = power;

        animationState.isDebuffing = true;
    }

    public void InitializeHill(Unit user, int power = 0)
    {
        Target.Clear();
        Target.Add(user);
        damage = power;

        animationState.isHiling = true;
    }

    public void StartRunAnimation()
    {
        if (anim != null)
        {
            anim.SetInteger("Run", 1);
        }
    }

    public void StopRunAnimation()
    {
        if (anim != null)
        {
            anim.SetInteger("Run", 0);
        }
    }

    public void AttackAnimation(int animationID = 1)
    {
        if (anim != null)
        {
            anim.SetInteger("Attack", animationID);
        }
    }

    public void BuffAnimation(int animationID)
    {
        if (anim != null)
        {
            anim.SetInteger("Buff", animationID);
        }
    }

    public void DebuffAnimation(int animationID)
    {
        if (anim != null)
        {
            anim.SetInteger("Debuff", animationID);
        }
    }

    public void HillAnimation(int animationID)
    {
        if (anim != null)
        {
            anim.SetInteger("Hill", animationID);
        }
    }

    // 攻撃アニメーション終了通知（AnimationEventで呼ばれる）
    public void OnAttackAnimationEnd()
    {
        Debug.Log("攻撃終わり―");
        anim.SetInteger("Attack", 0);
        animationState.isAttackAnimation = true;
        AnimationEnd();
    }

    public void HitAnimation()
    {
        if (animationState.ismultipleTaget)
        {
            Debug.Log("マルチターゲットヒットアニメーション開始");
            remaininghits = Target.Count;
            foreach (var ttt in Target)
            {
                AnimationController tgt = ttt.GetComponent<AnimationController>();
                Debug.Log("ヒットアニメーション開始");
                tgt.attacker = this;
                Animator animator = tgt.GetComponent<Animator>();

                if (animator != null)
                {
                    ttt.TakeDamage(damage, Im);//ダメージを受ける処理を先にすることで、生きてるか死んでるかで、どっちのアニメーションをするか分けれる。
                    if (ttt.status.currentHP <= 0)
                    {
                        //animator.SetInteger("Hit", 2); // 死亡アニメーション
                    }
                    else
                    {
                        animator.SetInteger("Hit", 1);
                    }    
                }
                else
                {
                    remaininghits--;
                    // 相手にアニメーションが無い場合 → 即完了扱い
                    Debug.Log($"【{tgt.name}】はAnimatorなし、即完了扱い");
                    ttt.TakeDamage(damage, Im);
                    if (remaininghits <= 0)
                    {
                        animationState.isHitAnimation = true; // 攻撃側のHit完了フラグ
                        AnimationEnd();        
                    }

                    //return;
                }
            }
        }
        else
        {
            Debug.Log("シングルターゲットヒットアニメーション開始");
            remaininghits = 1;
            AnimationController tgt = Target[0].GetComponent<AnimationController>();
            Debug.Log("ヒットアニメーション開始");
            tgt.attacker = this;
            Animator animator = tgt.GetComponent<Animator>();

            if (animator != null)
            {
                Target[0].TakeDamage(damage, Im);//ダメージを受ける処理を先にすることで、生きてるか死んでるかで、どっちのアニメーションをするか分けれる。
                if (Target[0].status.currentHP <= 0)
                {
                    //animator.SetInteger("Hit", 2); // 死亡アニメーション
                }
                else
                {
                    animator.SetInteger("Hit", 1);
                }   
            }
            else
            {
                // 相手にアニメーションが無い場合 → 即完了扱い
                Debug.Log($"【{tgt.name}】はAnimatorなし、即完了扱い");
                Target[0].TakeDamage(damage, Im);
                animationState.isHitAnimation = true; // 攻撃側のHit完了フラグ
                AnimationEnd();
                return;
            } 
        }


        // アニメーションある場合は通常処理
        //Target.TakeDamage(1);
    }

    // ヒットアニメーション終了通知（AnimationEventで呼ばれる）
    public void OnHitAnimationEnd()
    {

        anim.SetInteger("Hit", 0);
        Debug.Log("ヒット終わり―");

        if (attacker != null)
        {
            attacker.remaininghits--;
            if (attacker.remaininghits <= 0)
            {
                attacker.animationState.isHitAnimation = true;
                attacker.AnimationEnd();   
            }

        }
    }

    // 攻撃・ヒット両方終わった時に呼ぶ
    public void AnimationEnd()
    {
        if (!animationState.isAttackAnimation || !animationState.isHitAnimation)
            return;

        animationState.Reset();//変数を全部falseにisAttackingをfalseに

        anim.SetInteger("Attack", 0);

        Target.Clear();
        damage = 1;
        attacker = null;

        Debug.Log($"{name} both animations ended.");

        //onAnimationEnd?.Invoke();//Team.Playerだけ次のターンへ
        //プレイヤーの攻撃アニメーションだけ
        //プレイヤーとスキルのアニメーション
        //敵の攻撃アニメーション（現在は敵の攻撃アニメーションがないので使わないようになっている）
        //敵の両方のアニメーション
    }

    public void OnBuffAnimation()
    {
        Target[0].Buff(damage);
        //バフアニメーションを入れる。アタックで入れてる。
        //ヒットが終わった時と同じ処理をする。感じの
    }

    public void OnBuffAnimationEnd()
    {
        Debug.Log("バフアニメーション終わり―");
        anim.SetInteger("Buff", 0);
        animationState.isBuffing = false;
        
    }

    public void OnDebuffAnimation()
    {
        Target[0].Debuff(damage);
        //デバフアニメーションを入れる。アタックで入れてる。
        //ヒットが終わった時と同じ処理をする。感じの
    }

    public void OnDebuffAnimationEnd()
    {
        Debug.Log("デバフアニメーション終わり―");
        anim.SetInteger("Debuff", 0);
        animationState.isDebuffing = false;
        
    }

    public void OnHillAnimation()
    {
        Target[0].Heal(damage);
        //ヒールアニメーションを入れる。アタックで入れてる。
        //ヒットが終わった時と同じ処理をする。感じの
    }

    public void OnHillAnimationEnd()
    {
        Debug.Log("ヒールアニメーション終わり―");
        anim.SetInteger("Hill", 0);
        animationState.isHiling = false;
        
    }


}
