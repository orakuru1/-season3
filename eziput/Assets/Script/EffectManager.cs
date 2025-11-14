using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    public enum PlayerEffectType
    {
        Zangeki, // 斬撃エフェクト
        Fireball,
        Hit,
        Heal,
        GodHaniScicle
    }

    public GameObject zangekiEffectPrefab; // 斬撃エフェクトのプレハブ
    public GameObject MagicSicleEffectPrefab; // 魔法のサイクルエフェクトのプレハブ
    public GameObject HaniSicleEffectPrefab; 

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    /// <summary>
    /// 指定した種類のエフェクトを生成
    /// </summary>
    public void PlayEffect(PlayerEffectType type, Vector3 position, Transform transform, float time, Quaternion? rotation = null)
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
        if(type == PlayerEffectType.Zangeki)
        {
            GameObject sicle = Instantiate(MagicSicleEffectPrefab, position + transform.forward * 0.2f, yRotation);       
            Destroy(sicle, 2);     
        }


        // エフェクトの再生後に自動で削除
        Destroy(effect, time); // 1秒後に削除
        

    }

    /// <summary>
    /// EnumからPrefabを取得
    /// </summary>
    private GameObject GetEffectPrefab(PlayerEffectType type)
    {
        switch (type)
        {
            case PlayerEffectType.Zangeki:
                return zangekiEffectPrefab;
            case PlayerEffectType.Fireball:
                // return fireballEffectPrefab;
                return null;
            case PlayerEffectType.Hit:
                // return hitEffectPrefab;
                return null;
            case PlayerEffectType.Heal:
                // return healEffectPrefab;
                return null;
            case PlayerEffectType.GodHaniScicle:
                return HaniSicleEffectPrefab;
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
