using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// アニメーションの状態を管理する構造体
/// </summary>
[System.Serializable]
public class AnimationState
{
    public bool isAttacking;
    public bool isAttackAnimation;
    public bool isHitAnimation;
    public bool ismultipleTaget;
    public bool isBuffing;
    public bool isDebuffing;
    public bool isHiling;

    public void Reset()
    {
        isAttacking = false;
        isAttackAnimation = false;
        isHitAnimation = false;
        ismultipleTaget = false;
    }
}

