using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class RelicsManager : GenericSingletonClass<RelicsManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private RelicData[] allRelics;
    [SerializeField] private Relic relicPrefab;
    [SerializeField] private RelicData debugRelicData;

    [Header("Actions")]
    public Action<RelicData, int> OnRelicObtained;

    [Header("Private Infos")]
    private List<RelicData> currentAvailableRelics;

    [Header("Public Infos")]
    public bool[] PossessedRelicIndexes { get; private set; }
    public RelicData[] AllRelics { get { return allRelics; } }




    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Relic newRelic = Instantiate(relicPrefab, HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].transform.position, 
                Quaternion.Euler(0, 0, 0));
            newRelic.Initialise(debugRelicData);
        }
    }


    public void ObtainNewRelic(RelicData data)
    {
        for(int i = 0; i < allRelics.Length; i++)
        {
            if (allRelics[i] != data) continue;

            OnRelicObtained.Invoke(data, i);
            PossessedRelicIndexes[i] = true;
            currentAvailableRelics.Remove(allRelics[i]);

            break;
        }
    }


    public void StartExploration(int enviroIndex)
    {
        int startIndex = enviroIndex * 12;
        currentAvailableRelics = new List<RelicData>();

        for(int i = 0; i < 12; i++)
        {
            if (i + startIndex >= allRelics.Length) return;
            if (PossessedRelicIndexes[i + startIndex]) continue;

            currentAvailableRelics.Add(allRelics[i]);
        }
    }



    #region Verify Relic Spawn 

    public RelicData TryRelicSpawn(RelicSpawnType eventType, int floorIndex, float probaModificator)
    {
        RelicData[] possibeRelics = GetAllRelicsOfSpawnType(eventType);

        return GetSpawnedRelic(possibeRelics, floorIndex);
    }

    private RelicData[] GetAllRelicsOfSpawnType(RelicSpawnType spawnType)
    {
        List<RelicData> result = new List<RelicData>();

        for(int i = 0; i < currentAvailableRelics.Count; i++)
        {
            if (currentAvailableRelics[i].spawnType != spawnType && currentAvailableRelics[i].spawnType != RelicSpawnType.Everywhere) continue;

            result.Add(currentAvailableRelics[i]);
        }

        return result.ToArray();
    }

    private RelicData GetSpawnedRelic(RelicData[] possibleRelics, int floorIndex)
    {
        for(int i = 0;i < possibleRelics.Length; i++)
        {
            int pickedProba = Random.Range(0, 100);
            if (possibleRelics[i].spawnProbaPerFloor[floorIndex] > pickedProba)
            {
                return possibleRelics[i];
            }
        }

        return null;
    }

    #endregion


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
