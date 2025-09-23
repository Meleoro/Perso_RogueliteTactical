using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveData", menuName = "Scriptable Objects/PassiveData")]
public class PassiveData : ScriptableObject
{
    [Header("Main")]
    public string passiveName;
    [TextArea] public string passiveDescription;
    public Sprite passiveIcon;
    public Sprite passiveHighlightIcon;
    public AdditionalTooltipData[] additionalTooltipDatas;

    [Header("Effect")]
    public PassiveTriggerType passiveTriggerType;
    [Range(0, 100)] public float passiveTriggerProba;
    public PassiveEffect[] passiveEffects;
}

[Serializable]
public struct PassiveEffect
{
    public PassiveEffectType passiveEffectType;
    public AlterationData appliedAlteration;

    public int additivePower;
    public float multipliedPower;
    public int duration;

    public AIUnit summonPrefab;
}

public enum PassiveEffectType
{
    Alteration,
    MaxSkillPoints,
    GainSkillPoint
}

public enum PassiveTriggerType
{
    OnBattleStart,
    OnShieldGain,
    OnDamageReceived,
    OnAttack,
    OnKill
}
