using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSlotUiManeger : MonoBehaviour
{
    public static SkillSlotUiManeger instance{ get; private set; }
    public GameObject skillSlotUiPrefab;
    public Transform skillSlotUiParent;

    private List<GameObject> skillSlotUis = new List<GameObject>();
    void Awake()
    {
        if(instance == null) instance = this; else Destroy(gameObject);
    }

    public void CreateSkillSlotUi(List<SkillData> skills)
    {
        // 既存のスキルスロットUIを削除
        foreach(var ui in skillSlotUis)
        {
            Destroy(ui);
        }
        skillSlotUis.Clear();
        // 新しいスキルスロットUIを作成
        foreach(var skill in skills)
        {
            Debug.Log("スキルスロットUIを作成: " + skill.skillName);
            GameObject skillSlotUiObj = Instantiate(skillSlotUiPrefab, skillSlotUiParent);
            SkillSlotUI skillSlotUi = skillSlotUiObj.GetComponent<SkillSlotUI>();
            skillSlotUi.Initialize(skill);
            skillSlotUis.Add(skillSlotUiObj);
        }
    }
}
