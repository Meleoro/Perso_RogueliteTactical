using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PassivesManager 
{
    public bool VerifyAlterationImmunities(AlterationType alterationType, Unit unit)
    {
        for (int i = 0; i < unit.EquippedPassives.Length; i++)
        {
            if (unit.EquippedPassives[i] is null) continue;
            if (unit.EquippedPassives[i].passiveTriggerType != PassiveTriggerType.Always) continue;

            int pickedProba = Random.Range(0, 100);
            if (pickedProba >= unit.EquippedPassives[i].passiveTriggerProba) continue;

            if (unit.EquippedPassives[i].passiveEffects[0].passiveEffectType != PassiveEffectType.ImmuneAlteration) continue;
            if (unit.EquippedPassives[i].passiveEffects[0].appliedAlteration.alterationType != alterationType) continue;

            return true;
        }

        return false;
    }

    public int GetPassiveMaxSkillPointUpgrade(Unit unit)
    {
        for (int i = 0; i < unit.EquippedPassives.Length; i++)
        {
            if (unit.EquippedPassives[i] is null) continue;
            if (unit.EquippedPassives[i].passiveTriggerType != PassiveTriggerType.Always) continue;

            if (unit.EquippedPassives[i].passiveEffects[0].passiveEffectType != PassiveEffectType.MaxSkillPoints) continue;

            return (int)unit.EquippedPassives[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetPassiveCritChanceUpgrade(Unit unit, Unit attackedUnit)
    {
        for (int i = 0; i < unit.EquippedPassives.Length; i++)
        {
            if (unit.EquippedPassives[i] is null) continue;
            if (unit.EquippedPassives[i].passiveTriggerType != PassiveTriggerType.OnAttack && 
                (unit.EquippedPassives[i].passiveTriggerType != PassiveTriggerType.OnAttackUnitWithAlt || 
                !attackedUnit.VerifyHasAlteration(unit.EquippedPassives[i].neededAlteration.alterationType))) continue;

            if (unit.EquippedPassives[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeCritChances) continue;

            return (int)unit.EquippedPassives[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetGivePassiveAlterationUpgrade(Unit originUnit, AlterationData appliedAlteration)
    {
        for (int i = 0; i < originUnit.EquippedPassives.Length; i++)
        {
            if (originUnit.EquippedPassives[i] is null) continue;
            if (originUnit.EquippedPassives[i].passiveTriggerType != PassiveTriggerType.OnAlterationApplied) continue;

            if (originUnit.EquippedPassives[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeAlteration || 
                appliedAlteration.alterationType != originUnit.EquippedPassives[i].neededAlteration.alterationType) continue;

            return (int)originUnit.EquippedPassives[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetReceivePassiveAlterationUpgrade(Unit unit, AlterationData appliedAlteration)
    {
        for (int i = 0; i < unit.EquippedPassives.Length; i++)
        {
            if (unit.EquippedPassives[i] is null) continue;
            if (unit.EquippedPassives[i].passiveTriggerType != PassiveTriggerType.OnAlterationGained) continue;

            if (unit.EquippedPassives[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeAlteration ||
                appliedAlteration.alterationType != unit.EquippedPassives[i].neededAlteration.alterationType) continue;

            return (int)unit.EquippedPassives[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetPassiveCritDamagesUpgrade(Unit unit, Unit attackedUnit)
    {

        return 0;
    }


    public List<PassiveData> GetTriggeredPassives(PassiveTriggerType triggerType, Unit unit)
    {
        List<PassiveData> returnedList = new List<PassiveData>();

        if (!unit) return returnedList;

        for (int i = 0; i < unit.EquippedPassives.Length; i++)
        {
            if (unit.EquippedPassives[i] == null) continue;
            if (unit.EquippedPassives[i].passiveTriggerType != triggerType) continue;

            int pickedProba = Random.Range(0, 100);
            if (pickedProba >= unit.EquippedPassives[i].passiveTriggerProba) continue;

            returnedList.Add(unit.EquippedPassives[i]);
        }

        return returnedList;
    }

    public void ApplyPassives(PassiveData[] passivesToApply, Unit mainUnit, Unit secondaryUnit) 
    {
        for(int i = 0; i < passivesToApply.Length; i++)
        {
            ApplyPassive(passivesToApply[i].passiveEffects, mainUnit, secondaryUnit);
        }
    }

    private void ApplyPassive(PassiveEffect[] passivesEffects, Unit mainUnit, Unit secondaryUnit)
    {
        for(int i = 0; i < passivesEffects.Length; i++)
        {
            switch (passivesEffects[i].passiveEffectType)
            {
                case PassiveEffectType.ApplyOnSelfAlteration:
                    mainUnit.AddAlteration(passivesEffects[i].appliedAlteration, mainUnit);
                    break;

                case PassiveEffectType.ApplyOnOtherAlteration:
                    secondaryUnit.AddAlteration(passivesEffects[i].appliedAlteration, mainUnit);
                    break;

                case PassiveEffectType.UpgradeAlteration:
                    mainUnit.AddAlterationStrength(passivesEffects[i].appliedAlteration.alterationType,
                        passivesEffects[i].appliedAlteration.strength);
                    break;

                case PassiveEffectType.GainSkillPoint:
                    (mainUnit as Hero).AddSkillPoints(1);
                    break;

                case PassiveEffectType.MaxSkillPoints:
                    mainUnit.AddStatsModificators(0, 0, 0, 0, 0, (int)passivesEffects[i].additivePower);
                    break;

                case PassiveEffectType.UpgradeSummonHealth:
                    secondaryUnit.AddStatsModificators(secondaryUnit.CurrentMaxHealth + (int)passivesEffects[i].additivePower, secondaryUnit.CurrentStrength,
                        secondaryUnit.CurrentSpeed, secondaryUnit.CurrentLuck, 0, 0);
                    break;

                case PassiveEffectType.UpgradeSummonDamages:
                    secondaryUnit.ActualiseUnitInfos(secondaryUnit.CurrentMaxHealth, secondaryUnit.CurrentStrength + (int)passivesEffects[i].additivePower,
                        secondaryUnit.CurrentSpeed, secondaryUnit.CurrentLuck, 0, 0);
                    break;
            }
        }
    }
}
