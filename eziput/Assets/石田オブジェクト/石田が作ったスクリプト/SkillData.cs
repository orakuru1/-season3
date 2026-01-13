using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill", menuName = "skils")]
public class SkillData : ScriptableObject
{
    public int skillID;
    public string skillName;

    [Header("Unlock")]
    public WeaponType weaponType;
    public int requiredMastery; // 熟練度条件

    [Header("Input")]
    public KeyCode triggerKey;

    [Header("Battle")]
    public AttackPatternBase attackPattern;
    public int power;
    public int animationID;
    public int deathAnimationID;
}

[System.Serializable]
public class WeaponSkillLoadout
{
    public WeaponType weaponType;
    public List<SkillData> skills;
}

public enum WeaponType
{
    Sword,
    Axe,
    Spear,
    Bow,
    Staff,
    None
}

/// <summary>
/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>

[System.Serializable]
public class WeaponMasteryProgress
{
    public int level;
    public int exp;
    public int ExpToNextLevel = 5;


    public bool AddExp(int amount)
    {
        exp += amount;

        if (exp >= ExpToNextLevel)
        {
            exp -= ExpToNextLevel;
            level++;
            return true; // レベルアップした
        }

        return false;
    }
}

//武器種ごとにつける熟練度
[System.Serializable]
public class WeaponMasterySet
{
    public WeaponMasteryProgress sword;
    public WeaponMasteryProgress axe;
    public WeaponMasteryProgress spear;
    public WeaponMasteryProgress bow;
    public WeaponMasteryProgress staff;
    public WeaponMasteryProgress none;//素手用

    public void CopyFrom(WeaponMasterySet other)
    {
        sword.level = other.sword.level;
        sword.exp   = other.sword.exp;

        axe.level   = other.axe.level;
        axe.exp     = other.axe.exp;

        spear.level = other.spear.level;
        spear.exp   = other.spear.exp;

        bow.level   = other.bow.level;
        bow.exp     = other.bow.exp;

        staff.level = other.staff.level;
        staff.exp   = other.staff.exp;

        none.level  = other.none.level;
        none.exp    = other.none.exp;
    }

    public WeaponMasteryProgress Get(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:  return sword;
            case WeaponType.Axe:    return axe;
            case WeaponType.Spear:  return spear;
            case WeaponType.Bow:    return bow;
            case WeaponType.Staff:  return staff;
            case WeaponType.None:   return none;
            default: return null;
        }
    }
}
