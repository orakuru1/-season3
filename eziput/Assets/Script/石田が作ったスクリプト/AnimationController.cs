using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Unit Im;

    public AnimationController Target; // 攻撃する先
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

    public void Initialize(Unit target)
    {
        Target = target.GetComponent<AnimationController>();
        animationState.isAttacking = true;
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

    public void AttackAnimation()
    {
        if (anim != null)
        {
            anim.SetInteger("Attack", 1);
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
        Debug.Log("ヒットアニメーション開始");
        Target.attacker = this;
        Animator animator = Target.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetInteger("Hit", 1);
        }
        else
        {
            // 相手にアニメーションが無い場合 → 即完了扱い
            Debug.Log($"【{Target.name}】はAnimatorなし、即完了扱い");
            Target.GetComponent<Unit>().TakeDamage(1);
            animationState.isHitAnimation = true; // 攻撃側のHit完了フラグ
            AnimationEnd();
            return;
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

        Target = null;
        attacker = null;

        Debug.Log($"{name} both animations ended.");

        onAnimationEnd?.Invoke();
    }


}
