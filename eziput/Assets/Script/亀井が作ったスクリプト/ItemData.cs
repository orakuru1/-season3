using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ///////////////////////////////////////////////////////////////////////////
/// </summary>

[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public ItemCategory category;
    public Sprite icon;
}

public enum ItemCategory
{
    Item,
    Weapon,
    Armor
}
