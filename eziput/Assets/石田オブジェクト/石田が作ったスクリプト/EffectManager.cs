using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    public List<AttackEffects> attackEffects = new List<AttackEffects>();
    public List<HitEffects> hitEffects = new List<HitEffects>();
    public List<HealEffect> healEffects = new List<HealEffect>();
    public List<DebuffEffects> debuffEffects = new List<DebuffEffects>();
    public List<BuffEffects> buffEffects = new List<BuffEffects>();


    private Dictionary<int, AttackEffects> attackEffectMap = new Dictionary<int, AttackEffects>();
    private Dictionary<int, HitEffects> hitEffectMap = new Dictionary<int, HitEffects>();
    private Dictionary<int, HealEffect> healEffectMap = new Dictionary<int, HealEffect>();
    private Dictionary<int, DebuffEffects> debuffEffectMap = new Dictionary<int, DebuffEffects>();
    private Dictionary<int, BuffEffects> buffEffectMap = new Dictionary<int, BuffEffects>();


    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);

        foreach(var effect in hitEffects)
        {
            if(!hitEffectMap.ContainsKey(effect.id))
            hitEffectMap.Add(effect.id, effect);
        }

        foreach(var effect in attackEffects)
        {
            if(!attackEffectMap.ContainsKey(effect.id))
            attackEffectMap.Add(effect.id, effect);
        }

        foreach(var effect in healEffects)
        {
            if(!healEffectMap.ContainsKey(effect.id))
            healEffectMap.Add(effect.id, effect);
        }

        foreach(var effect in debuffEffects)
        {
            if(!debuffEffectMap.ContainsKey(effect.id))
            debuffEffectMap.Add(effect.id, effect);
        }

        foreach(var effect in buffEffects)
        {
            if(!buffEffectMap.ContainsKey(effect.id))
            buffEffectMap.Add(effect.id, effect);
        }

    }

    /// <summary>
    /// 指定した種類のエフェクトを生成
    /// </summary>
    public void PlayAttackEffect(Vector3 pos, Transform origin, int id)
    {
        if(!attackEffectMap.TryGetValue(id, out AttackEffects data))
        {
            Debug.LogWarning($"AttackEffect ID {id} が見つかりません");
            return;
        }

        Quaternion rot = data.alignToCharacter? Quaternion.Euler(0, origin.eulerAngles.y, 0) : Quaternion.identity;

        Vector3 finalPos = pos + data.offset;

        GameObject effect = Instantiate(data.effectPrefab, finalPos, rot);
        Destroy(effect, data.duration);

        // 追加エフェクト処理（Zangeki など）
        if (data.spawnExtraEffect && data.extraEffectPrefab != null)
        {
            if (data.useCharacterForward)
            {
                pos += origin.forward * data.forwardOffset;
            }
            Vector3 extraPos = pos + data.extraOffset;
            GameObject extra = Instantiate(data.extraEffectPrefab, extraPos, rot);
            Destroy(extra, data.duration);
        }

    }


    public void PlayHitEffect(Vector3 pos, int id)
    {
        if (!hitEffectMap.TryGetValue(id, out HitEffects data))
        {
            Debug.LogWarning($"HitEffect ID {id} が見つかりません");
            return;
        }

        GameObject effect = Instantiate(data.effectPrefab, pos + data.offset, Quaternion.identity);
        Destroy(effect, data.duration);
    }

    public void PlayBuffEffect(Vector3 pos, int id)
    {
        if (!buffEffectMap.TryGetValue(id, out BuffEffects data))
        {
            Debug.LogWarning($"BuffEffect ID {id} が見つかりません");
            return;
        }

        GameObject effect = Instantiate(data.effectPrefab, pos + data.offset, Quaternion.identity);
        Destroy(effect, data.duration);
    }

    public void PlayDebuffEffect(Vector3 pos, int id)
    {
        if (!debuffEffectMap.TryGetValue(id, out DebuffEffects data))
        {
            Debug.LogWarning($"DebuffEffect ID {id} が見つかりません");
            return;
        }

        GameObject effect = Instantiate(data.effectPrefab, pos + data.offset, Quaternion.identity);
        Destroy(effect, data.duration);
    }

    public void PlayHillEffect(Vector3 pos, int id)
    {
        if (!healEffectMap.TryGetValue(id, out HealEffect data))
        {
            Debug.LogWarning($"HillEffect ID {id} が見つかりません");
            return;
        }

        GameObject effect = Instantiate(data.effectPrefab, pos + data.offset, Quaternion.identity);
        Destroy(effect, data.duration);
    }
}
