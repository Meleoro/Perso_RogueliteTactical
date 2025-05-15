using System;
using System.Collections.Generic;
using UnityEngine;


public enum SkillType
{
    Self,
    AOEPaternTiles,
    SkillArea
}

public enum SkillEffectTargetType
{
    Self,
    Allies,
    Enemies
}

public enum SkillEffectType
{
    Damage,
    Heal,
    ModifyStrength,
    ModifySpeed,
    ModifyLuck,
    ModifyMove,
    AddShield,
    AddEnergy,
    Push,
    Provoke
}

[Serializable]
public struct SkillEffect
{
    public SkillEffectType skillEffectType;
    public SkillEffectTargetType skillEffectTargetType;

    public int additivePower;
    public float multipliedPower;
    public int duration;
}


[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public string animName;
    public int skillPointCost;

    public SkillType skillType;
    public SkillEffect[] skillEffects;

    public bool useOrientatedAOE;
    public bool[] skillPatern = new bool[15 * 15];
    public bool[] skillAOEPatern = new bool[9 * 9];
    public bool[] skillAOEPaternHorizontal = new bool[9 * 9];
    public bool[] skillAOEPaternVertical = new bool[9 * 9];
}
