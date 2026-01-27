using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/WeaponData")]
public class WeaponData : ItemData
{
    public WeaponType weaponType;
    public int attackBonus;
    public float varianceBonus;//どのくらいの武器の振れ幅
    public int durability;//上昇する熟練度
    public WeaponVisualData weaponPrefab;
}
