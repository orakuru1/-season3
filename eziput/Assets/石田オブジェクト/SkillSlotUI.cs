using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Text skillNameText;
    public Text skillKeyText;
    public Image skillIconImage;

    public void Initialize(SkillData skillData)
    {
        skillNameText.text = skillData.skillName;
        skillKeyText.text = skillData.triggerKey.ToString();
        // skillIconImage.sprite = skillData.icon; // Assuming SkillData has an icon property
    }
}
