using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int uniqueId;  //アイテムを一意に識別
    public string name;   //名称
    public Sprite icon;  //アイコン
    public int atk;      //攻撃力
    public int def;      //防御力
}

public class ItemHolder : MonoBehaviour
{
    public ItemData data;
}
