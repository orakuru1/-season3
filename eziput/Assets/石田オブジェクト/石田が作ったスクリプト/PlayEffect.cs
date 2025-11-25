using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayEffect : MonoBehaviour
{
    public void zangekiEffect(int id)
    {
        EffectManager.Instance.PlayAttackEffect(transform.position, transform, id);
    }

    public void GodSicleEffect(int id)
    {
        EffectManager.Instance.PlayAttackEffect(transform.position, transform, id);
    }

    public void LarseEnemyUpEffect(int id)
    {
        foreach(Vector3 pos in GodManeger.Instance.enemyPosition)
        {
            EffectManager.Instance.PlayAttackEffect(pos, transform, id);
        } 
    }

    public void BlueHitEffect(int id)
    {
        foreach(Vector3 pos in GodManeger.Instance.enemyPosition)
        {
            EffectManager.Instance.PlayHitEffect(pos, id);
        } 
    }

    public void BuffEffect(int id)
    {
        EffectManager.Instance.PlayBuffEffect(transform.position, id);
    }

    public void DebuffEffect(int id)
    {
        foreach(Vector3 pos in GodManeger.Instance.enemyPosition)
        {
            EffectManager.Instance.PlayDebuffEffect(pos, id);
        } 
    }

    public void HealEffect(int id)
    {
        EffectManager.Instance.PlayHillEffect(transform.position, id);
    }
}







