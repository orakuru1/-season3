using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Unit Im;

    public AnimationController Target; // 攻撃する先
    public AnimationController attacker; // 攻撃してきた人

    public bool isAttacking = false;//下３つを違うスクリプトに移動予定
    public bool isAttackAnimation = false;
    public bool isHitAnimation = false;

    private Animator anim;

    public Unit.Team team;

    void Start()
    {
        Im = GetComponent<Unit>();
        team = Im.team;
        anim = GetComponent<Animator>();
    }

    public void Initialize(Unit target)
    {
        Target = target.gameObject.GetComponent<AnimationController>();
        isAttacking = true;
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
    private void OnAttackAnimationEnd()
    {
        Debug.Log($"{this.name} attack animation ended.");
        isAttackAnimation = true;
        AnimationEnd();
    }

    private void HitAnimation()
    {
        Target.attacker = this;
        Animator animator = Target.gameObject.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetInteger("Hit", 1);
        }
        else
        {
            // 相手にアニメーションが無い場合 → 即完了扱い
            Debug.Log($"【{Target.name}】はAnimatorなし、即完了扱い");
            Target.gameObject.GetComponent<Unit>().TakeDamage(1);
            isHitAnimation = true; // 攻撃側のHit完了フラグ
            AnimationEnd();
            return;
        }

        // アニメーションある場合は通常処理
        //Target.TakeDamage(1);
    }

    // ヒットアニメーション終了通知（AnimationEventで呼ばれる）
    private void OnHitAnimationEnd()
    {
        anim.SetInteger("Hit", 0);
        Debug.Log($"{this.name} hit animation ended.");

        if (attacker != null)
        {
            attacker.isHitAnimation = true;
            attacker.AnimationEnd();
        }
    }

    // 攻撃・ヒット両方終わった時に呼ぶ
    public void AnimationEnd()
    {
        if (!isAttackAnimation || !isHitAnimation)
            return;

        isAttackAnimation = false;
        isHitAnimation = false;
        isAttacking = false;

        anim.SetInteger("Attack", 0);

        Target = null;
        attacker = null;

        Debug.Log($"{name} both animations ended.");

        // 攻撃終了 → ターン進行
        if (team == Unit.Team.Player)
        {
            TurnManager.Instance.NextTurn();
        }
    }


}
