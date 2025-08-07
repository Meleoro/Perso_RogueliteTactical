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
    public EnemySpawnData[] enemySpawnsPerFloor;
}
