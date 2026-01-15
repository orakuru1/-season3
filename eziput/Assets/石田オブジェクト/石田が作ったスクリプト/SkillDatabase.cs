using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Database/SkillDatabase")]
public class SkillDatabase : ScriptableObject
{
    public WeaponSkillPoolSet normalSkills;
    public List<AttackSkill> rareSkills;//レベルアップ以外で覚える技

    private Dictionary<int, SkillData> skillDict;

    public void BuildDictionary()
    {
        skillDict = new Dictionary<int, SkillData>();

        foreach(var list in GetAllSkillLists())
        {
            foreach(var skill in list)
            {
                if(!skillDict.ContainsKey(skill.skillID))
                {
                    skillDict.Add(skill.skillID, skill);
                }
            }
            
        }
    }

    public SkillData GetById(int id)
    {
        if(skillDict.TryGetValue(id, out SkillData skill))
        {
            return skill;
        }
        Debug.LogWarning($"SkillDatabase: ID {id} のスキルが見つかりません。");
        return null;
    }

    private List<List<SkillData>> GetAllSkillLists()
    {
        return new List<List<SkillData>>()
        {
            normalSkills.sword,
            normalSkills.axe,
            normalSkills.spear,
            normalSkills.bow,
            normalSkills.staff,
            normalSkills.none,
            rareSkills.Cast<SkillData>().ToList()
        };
    }
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
