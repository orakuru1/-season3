using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Unit Im;

    public List<Unit> Target = new List<Unit>(); // 攻撃する先
    public int damage;
    public AnimationController attacker; // 攻撃してきた人
    public AnimationState animationState = new AnimationState();//条件を構造体にまとめた

    private Animator anim;

    public Unit.Team team;

    public System.Action onAnimationEnd;// アニメーション終了時に呼ばれるイベント

    void Start()
    {
        Im = GetComponent<Unit>();
        team = Im.team;
        anim = GetComponent<Animator>();
    }

    public void Initialize(Unit target, int dmg = 1)
    {
        Target.Clear();
        Target.Add(target);
        damage = dmg;
        animationState.isAttacking = true;
        animationState.ismultipleTaget = false;
    }
    public void Initialize(List<Unit> targets, int dmg = 1)
    {
        Target = targets;
        damage = dmg;
        animationState.isAttacking = true;
        animationState.ismultipleTaget = true;
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

    // 攻撃アニメーション終了通知（AnimationEventで呼ばれる）
    public void OnAttackAnimationEnd()
    {
        Debug.Log("攻撃終わり―");
        animationState.isAttackAnimation = true;
        AnimationEnd();
    }

    public void HitAnimation()
    {
        if (animationState.ismultipleTaget)
        {
            Debug.Log("マルチターゲットヒットアニメーション開始");
            foreach (var ttt in Target)
            {
                AnimationController tgt = ttt.GetComponent<AnimationController>();
                Debug.Log("ヒットアニメーション開始");
                tgt.attacker = this;
                Animator animator = tgt.GetComponent<Animator>();

                if (animator != null)
                {
                    ttt.TakeDamage(damage);//ダメージを受ける処理を先にすることで、生きてるか死んでるかで、どっちのアニメーションをするか分けれる。
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
                    // 相手にアニメーションが無い場合 → 即完了扱い
                    Debug.Log($"【{tgt.name}】はAnimatorなし、即完了扱い");
                    ttt.TakeDamage(damage);
                    animationState.isHitAnimation = true; // 攻撃側のHit完了フラグ
                    AnimationEnd();
                    //return;
                }
            }
        }
        else
        {
            Debug.Log("シングルターゲットヒットアニメーション開始");
            AnimationController tgt = Target[0].GetComponent<AnimationController>();
            Debug.Log("ヒットアニメーション開始");
            tgt.attacker = this;
            Animator animator = tgt.GetComponent<Animator>();

            if (animator != null)
            {
                Target[0].TakeDamage(damage);//ダメージを受ける処理を先にすることで、生きてるか死んでるかで、どっちのアニメーションをするか分けれる。
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
                Target[0].TakeDamage(damage);
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
            attacker.animationState.isHitAnimation = true;
            attacker.AnimationEnd();
        }
    }

    // 攻撃・ヒット両方終わった時に呼ぶ
    public void AnimationEnd()
    {
        if (!animationState.isAttackAnimation || !animationState.isHitAnimation)
            return;

        animationState.Reset();

        anim.SetInteger("Attack", 0);

        Target.Clear();
        attacker = null;

        Debug.Log($"{name} both animations ended.");

        onAnimationEnd?.Invoke();
    }


}
