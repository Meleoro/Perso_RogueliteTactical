using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorLootData", menuName = "Scriptable Objects/FloorLootData")]
public class FloorLootData : ScriptableObject
{
    [Header("Normal Chests")]
    public PossibleLootData[] chestPossibleLoots;
    public int minChestCoins;
    public int maxChestCoins;

    [Header("Battle Ends")]
    public PossibleLootData[] battleEndPossibleLoots;
    public int minBattleCoins;
    public int maxBattleCoins;

    [Header("Challenge")]
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
