using UnityEngine;

[CreateAssetMenu(fileName = "RelicData", menuName = "Scriptable Objects/RelicData")]
public class RelicData : ScriptableObject
{
    [Header("Main Infos")]
    public string relicName;
    [TextArea] public string relicDescription;
    [TextArea] public string relicHint;
    public Sprite icon;
    public RarityType type;

    [Header("Spawn Infos")]
    public RelicSpawnType spawnType;
    public float[] spawnProbaPerFloor;
}

public enum RelicSpawnType
{
    BattleEndSpawn,
    BossBattleEndSpawn,
    TrialChestSpawn,
    NormalChestSpawn
}

public enum RarityType
{
    Common, 
    Rare,
    Epic, 
    Mystic
}