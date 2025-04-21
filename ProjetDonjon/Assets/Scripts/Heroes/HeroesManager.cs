using UnityEngine;

public class HeroesManager : MonoBehaviour
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
            heroes[i].SetupInventory(_inventoryManager.InitialiseInventory(heroes[i].heroInventoryPrefab, i));
        }

        _interactionManager.ActualiseCurrentHeroTransform(heroes[0].transform);
        CameraManager.Instance.Initialise(heroes[0].transform);
    }

    private void SwitchHero()
    {
        currentHeroIndex = ++currentHeroIndex % heroes.Length;


    }
}
