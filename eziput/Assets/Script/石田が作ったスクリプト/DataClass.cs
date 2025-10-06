using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataClass : MonoBehaviour
{

}

[System.Serializable]
public class StatusSaveData
{
    public string idname;
    public string characterName;//キャラクターの名前
    public int HP;
    public int maxHP;
    public float speed;
}

[System.Serializable]
public class SaveData
{
    public List<StatusSaveData> statuss = new List<StatusSaveData>();
    public int floarLevel;

}
