using System;
using UnityEngine;


[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroData")]
public class HeroData : UnitData
{
    [Header("Hero Infos")]
    public int baseMovePoints;
    public int maxSkillPoints;
    public int startSkillPoints;
    public Inventory heroInventoryPrefab;

    [Header("Hero Skills")]
    public SkillData[] heroBaseSkills;
    public SkillData[] heroSkillPool;
    public PassiveData[] heroPassivePool;
    public int[] heroSkillSlotsUnlockedLevels = new int[6];
    public int[] heroPassiveSlotsUnlockedLevels = new int[3];
}
