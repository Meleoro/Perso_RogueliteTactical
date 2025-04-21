using System;
using UnityEngine;

public enum LootType
{
    Treasure,
    Equipment,
    Consumable,
    Relic
}

public enum EquipmentType
{
    Weapon,
    Headset,
    Breast,
    Legs,
    Ring
}


[CreateAssetMenu(fileName = "LootData", menuName = "Scriptable Objects/LootData")]
public class LootData : ScriptableObject
{
    [Header("Main Infos")]
    public string lootName;
    public string lootDescription;
    public LootType lootType;
    public Sprite sprite;
    public int value;
    public SpaceTakenRow[] spaceTaken;

    [Header("Equipment Infos")]
    public EquipmentType equipmentType;
    public Sprite equipmentSprite;
    public int healthUpgrade;
    public int strengthUpgrade;
    public int speedUpgrade;
    public int luckUpgrade;
}

[Serializable]
public struct SpaceTakenRow
{
    public bool[] row;
}
