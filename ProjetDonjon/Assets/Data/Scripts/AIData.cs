using UnityEngine;


public enum AIType
{
    Classic,
    Shy
}

[CreateAssetMenu(fileName = "AIData", menuName = "Scriptable Objects/AIData")]
public class AIData : UnitData
{
    [Header("Enemy Data")]
    public AIType AI;
    public int dangerLevel;
    public SkillData[] skills;
    public bool[] movePatern = new bool[15 * 15];

}
