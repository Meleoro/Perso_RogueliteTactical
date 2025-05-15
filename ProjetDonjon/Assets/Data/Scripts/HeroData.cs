using System;
using UnityEngine;

[Serializable]
public struct HeroSkillStruct
{
    public SkillData skill;
    public int levelToUnlock;
}

[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroData")]
public class HeroData : UnitData
{
    [Header("Hero Infos")]
    public int baseMovePoints;
    public int maxSkillPoints;
    public int startSkillPoints;
    public Inventory heroInventoryPrefab;

    [Header("Hero Skills")]
    public HeroSkillStruct[] heroSkills;
}
