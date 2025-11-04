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

    public void Reset()
    {
        isAttacking = false;
        isAttackAnimation = false;
        isHitAnimation = false;
        ismultipleTaget = false;
    }
}



/// <summary>
/// 仮のキャラステータス
/// </summary>
[System.Serializable]
public class StatusSaveData
{
    public string idname;
    public string characterName;//キャラクターの名前
    public int HP;
    public int maxHP;
    public float speed;
}
/// <summary>
/// いずれ保存しないといけないデータをまとめるクラス
/// </summary>

[System.Serializable]
public class PyramidData
{
    public int currentFloor;      // 現在の階層
    public bool[] clearedFloors;  // 各階層のクリア状態
}

[System.Serializable]
public class ProgressData
{
    public string currentScene;   // シーン名
    public string checkpointID;   // チェックポイントやイベントID
    public float playTime;        // 総プレイ時間など
}
/// <summary>
/// ↑のここまでをまとめるクラス
/// </summary>

[System.Serializable]
public class SaveData
{
    public List<StatusSaveData> statuss = new List<StatusSaveData>();
    public PyramidData pyramid;
    //public List<GodData> gods;
    public ProgressData progress;
}
