using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    public enum EffectType
    {
        Zangeki, // 斬撃エフェクト
        Fireball,
        Hit,
        Heal,
    }
    public GameObject zangekiEffectPrefab; // 斬撃エフェクトのプレハブ
    public GameObject MagicSicleEffectPrefab; // 魔法のサイクルエフェクトのプレハブ

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    /// <summary>
    /// 指定した種類のエフェクトを生成
    /// </summary>
    public void PlayEffect(EffectType type, Vector3 position, Transform transform, Quaternion? rotation = null)
    {
        GameObject prefab = GetEffectPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"❌ Effect prefab not assigned for {type}");
            return;
        }
        // キャラクターのY軸だけを取得
        Quaternion yRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);//y軸の回転を取得
        Quaternion rot = rotation ?? Quaternion.identity;//指定がなければ回転なし
        GameObject effect = Instantiate(prefab, position, rot);
        GameObject sicle = Instantiate(MagicSicleEffectPrefab, position + transform.forward * 0.2f, yRotation);

        // エフェクトの再生後に自動で削除
        Destroy(effect, 1f); // 1秒後に削除
        Destroy(sicle, 1.4f);

    }

    /// <summary>
    /// EnumからPrefabを取得
    /// </summary>
    private GameObject GetEffectPrefab(EffectType type)
    {
        switch (type)
        {
            case EffectType.Zangeki:
                return zangekiEffectPrefab;
            case EffectType.Fireball:
                // return fireballEffectPrefab;
                return null;
            case EffectType.Hit:
                // return hitEffectPrefab;
                return null;
            case EffectType.Heal:
                // return healEffectPrefab;
                return null;
            default:
                return null;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
