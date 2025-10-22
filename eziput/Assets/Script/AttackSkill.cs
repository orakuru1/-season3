using UnityEngine;

[CreateAssetMenu(menuName = "Attack/AttackSkill")]
public class AttackSkill : ScriptableObject
{
    public string skillName = "New Skill";
    public AttackPatternBase attackPattern;
    public int power = 1;
    public KeyCode key = KeyCode.J; // Ç±ÇÃÉLÅ[Ç≈î≠ìÆ
}
