using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Drop/Enemy Drop Table")]
public class EnemyDropTable : ScriptableObject
{
    public List<DropCategory> dropCategories;
}

[System.Serializable]
public class DropItem
{
    public string itemName;
    [Range(0f, 1f)]
    public float dropRate;   // このアイテム自体の確率
    public int minCount = 1;
    public int maxCount = 1;
}

[System.Serializable]
public class DropCategory
{
    public string categoryName; // "Normal", "Rare" など
    [Range(0f, 1f)]
    public float categoryRate;  // このカテゴリが抽選される確率
    public List<DropItem> items;
}
