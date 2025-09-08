using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class RelicsManager : GenericSingletonClass<RelicsManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] public RelicData[] allRelics;

    [Header("Private Infos")]
    private List<RelicData> currentAvailableRelics;

    [Header("Public Infos")]
    public bool[] PossessedRelicIndexes { get; private set; }
    public RelicData[] AllRelics { get { return allRelics; } }




    public RelicData TryBattleEndSpawn(int floorIndex, float probaModificator)
    {
        return null;
    }

    public RelicData TryBossBattleEndSpawn(int floorIndex, float probaModificator)
    {
        return null;
    }


    public void AddRelic(int relicIndex)
    {
        PossessedRelicIndexes[relicIndex] = true;
    }


    #region Save Interface
    public void LoadGame(GameData data)
    {
        PossessedRelicIndexes = data.possessedRelicsIndexes;
    }

    public void SaveGame(ref GameData data)
    {
        data.possessedRelicsIndexes = PossessedRelicIndexes;
    }

    #endregion
}
