using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class HeroesManager : GenericSingletonClass<HeroesManager>
{
    [Header("Debug")]
    [SerializeField] private Hero[] debugHeroes;
    [SerializeField] private Transform debugSpawnPos;

    [Header("Public Infos")]
    public Hero[] Heroes { get { return heroes; } }
    public int CurrentHeroIndex { get { return currentHeroIndex; } }

    [Header("Private Infos")]
    private Hero[] heroes;
    private int currentHeroIndex;

    [Header("References")]
    [SerializeField] private InteractionManager _interactionManager;
    [SerializeField] private InventoriesManager _inventoryManager;



    public void Initialise(Hero[] heroesPrefabs, Vector2 spawnPos)
    {
        if(heroesPrefabs is null)
        {
            heroesPrefabs = debugHeroes;
        }

        heroes = new Hero[heroesPrefabs.Length];

        for (int i = 0; i < heroesPrefabs.Length; i++)
        {
            heroes[i] = Instantiate(heroesPrefabs[i], spawnPos, Quaternion.Euler(0, 0, 0));
            heroes[i].SetupInventory(_inventoryManager.InitialiseInventory(heroes[i].HeroData.heroInventoryPrefab, i));
        }

        _interactionManager.ActualiseCurrentHeroTransform(heroes[0].transform);
        CameraManager.Instance.Initialise(heroes[0].transform);

        ActualiseDisplayedHero();

        UIManager.Instance.SetupHeroInfosPanel(heroes);
    }


    private void SwitchHero()
    {
        currentHeroIndex = ++currentHeroIndex % heroes.Length;

        ActualiseDisplayedHero();
    }


    private void ActualiseDisplayedHero()
    {
        for (int i = 0; i < heroes.Length; i++)
        {
            heroes[i].ActualiseCurrentDisplayedHero(heroes[currentHeroIndex].transform);

            if (currentHeroIndex == i)
            {
                heroes[i].ShowHero();
            }
            else
            {
                heroes[i].HideHero();
            }
        }
    }



    #region Battle Functions

    // Places the 3 heroes on te nearest tiles and initialises there battle behavior
    public void EnterBattle(List<BattleTile> possibleTiles)
    {
        for(int i = 0; i < heroes.Length; i++)
        {
            BattleTile pickedTile = possibleTiles[0];
            float bestDist = Mathf.Infinity;

            for (int j = 0; j < possibleTiles.Count; j++)
            {
                if (possibleTiles[j].UnitOnTile is not null) continue;

                float currentDist = Vector2.Distance(possibleTiles[j].transform.position, heroes[i].transform.position);
                if (currentDist < bestDist)
                {
                    pickedTile = possibleTiles[j];
                    bestDist = currentDist;
                }
            }

            pickedTile.UnitEnterTile(heroes[i]);
            heroes[i].EnterBattle(pickedTile);

            heroes[i].ShowHero();
        }
    }

    public void ExitBattle()
    {
        for (int i = 0; i < heroes.Length; i++)
        {
            heroes[i].ExitBattle(heroes[currentHeroIndex]);
        }
    }

    #endregion
}
