using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnviroData", menuName = "Scriptable Objects/EnviroData")]
public class EnviroData : ScriptableObject
{
    [Header("Main Parameters")]
    public int minDistGoldenPath;
    public int maxDistGoldenPath;
    public int minRoomAmount;
    public int maxRoomAmount;
    public int floorsAmount;

    [Header("Base Rooms")]
    public Room[] possibleBattleRooms;
    public Room[] possibleCorridorRooms;
    public Room[] possibleCorridorTrapRooms;
    public Room[] possibleCorridorPlateformRooms;
    public Room[] possiblePlateformRooms;
    public Room[] possibleTrapRooms;
    public Room[] possiblePuzzleRooms;
    public Room[] possibleChallengeRooms;

    [Header("Special Rooms")]
    public Room[] possibleStartRooms;
    public Room[] possibleStairsRooms;
    public Room[] possibleBossRooms;

    [Header("Loot")]
    public FloorLootData[] lootPerFloors;
}

[Serializable]
public class FloorLootData
{
    public PossibleLootData[] chestPossibleLoots;
    public PossibleLootData[] battleEndPossibleLoots;
    public PossibleLootData[] challengeEndPossibleLoots;
}

[Serializable]
public class PossibleLootData
{
    [Range(0, 100)] public int probability;
    public LootData loot;
}
