using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeData", menuName = "Scriptable Objects/SkillTreeData")]
public class SkillTreeData : ScriptableObject
{
    public SkillTreeRow[] skillTreeRows;
}


[Serializable]
public struct SkillTreeRow
{
    public SkillTreeNodeData[] rowNodes;
}

[Serializable]
public struct SkillTreeNodeData
{
    public SkillData skillData;
    public PassiveData passiveData;
    public Sprite icon;

    public int[] connectedNextNodeIndexes;
}
