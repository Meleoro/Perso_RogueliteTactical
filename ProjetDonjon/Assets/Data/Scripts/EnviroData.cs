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
    public Room[] possibleFirstBossRooms;
    public Room[] possibleSecondBossRooms;

    [Header("Loot")]
    public FloorLootData[] lootPerFloors;
    public EnemySpawnsPerFloor[] enemySpawnsPerFloor;
}

[Serializable]
public class FloorLootData
{
    public PossibleLootData[] chestPossibleLoots;
    public int minChestCoins;
    public int maxChestCoins;

    public PossibleLootData[] battleEndPossibleLoots;
    public int minBattleCoins;
    public int maxBattleCoins;

    public PossibleLootData[] challengeEndPossibleLoots;
    public int minChallengeCoins;
    public int maxChallengeCoins;
}

[Serializable]
public class PossibleLootData
{
    [Range(0, 100)] public int probability;
    public LootData loot;
}


[Serializable]
public class EnemySpawnsPerFloor
{
    public EnemySpawn[] possibleEnemies;
}

[Serializable]
public class EnemySpawn
{
    public Unit enemyPrefab;
    [Range(0, 100)] public int proba;
    [Range(0, 100)] public int eliteProba;
    public int maxCountPerBattle;
    public int minEnemyCountBeforeSpawn;
}
