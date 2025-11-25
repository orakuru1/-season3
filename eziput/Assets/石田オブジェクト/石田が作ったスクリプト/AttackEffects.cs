using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackEffect", menuName = "AttackEffects")]
public class AttackEffects : ScriptableObject
{
    public int id;
    public GameObject effectPrefab;
    public float duration = 1f;
    public Vector3 offset = new Vector3(0, 0, 0);

    public bool alignToCharacter = false;       // キャラクターの向きに合わせるかどうか
    public bool spawnExtraEffect = false;       // 追加エフェクトを生成するかどうか
    public GameObject extraEffectPrefab;        // 追加エフェクトのプレハブ
    public Vector3 extraOffset;                 // 追加エフェクトのオフセット

    public bool useCharacterForward = false;    // キャラクターの前方方向にオフセットを適用するかどうか
    public float forwardOffset;                 // キャラクターの前方方向へのオフセット量
}
