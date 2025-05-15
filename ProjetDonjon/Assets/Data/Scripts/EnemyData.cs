using UnityEngine;


public enum AIType
{
    Classic,
    Shy
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : UnitData
{
    [Header("Enemy Data")]
    public AIType AI;
    public int dangerLevel;
    public SkillData[] skills;
    public bool[] movePatern = new bool[15 * 15];

}
