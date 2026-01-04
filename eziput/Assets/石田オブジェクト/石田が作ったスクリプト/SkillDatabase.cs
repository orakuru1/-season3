using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/SkillDatabase")]
public class SkillDatabase : ScriptableObject
{
    public WeaponSkillPoolSet normalSkills;
    public List<AttackSkill> rareSkills;//レベルアップ以外で覚える技
}

[System.Serializable]
public class WeaponSkillPoolSet
{
    public List<SkillData> sword;
    public List<SkillData> axe;
    public List<SkillData> spear;
    public List<SkillData> bow;
    public List<SkillData> staff;
    public List<SkillData> none;

    public List<SkillData> Get(WeaponType type)
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
