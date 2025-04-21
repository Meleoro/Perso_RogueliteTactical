using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utilities;

public enum UIState
{
    Nothing,
    Inventories,
    HeroesInfos
}

public class UIManager : GenericSingletonClass<UIManager>
{
    [Header("Public Infos")]
    public Loot DraggedLoot { 
        get { return draggedLoot; } 
        set 
        { 
            draggedLoot = value;
            if (draggedLoot == null) OnStopDrag?.Invoke();
            else OnStartDrag?.Invoke();
        } 
    }

    [Header("Actions")]
    public Action OnStartDrag;
    public Action OnStopDrag;

    [Header("Private Infos")]
    private UIState currentState;
    private Loot draggedLoot;

    [Header("References")]
    [SerializeField] private InventoriesManager _inventoriesManager;
    [SerializeField] private HeroInfosScreen _heroInfosScreen;


    private void Start()
    {
        _inventoriesManager.OnInventoryOpen += OpenInventory;
    }

    private void Update()
    {
        switch (currentState)
        {
            case UIState.Nothing:
                if (InputManager.wantsToHeroInfo)
                {
                    if (!_heroInfosScreen.VerifyCanOpenOrCloseHeroInfos()) return;
                    StartCoroutine(_heroInfosScreen.OpenInfosScreenCoroutine());
                    currentState = UIState.HeroesInfos;
                }

                if (InputManager.wantsToInventory)
                {
                    if (!_inventoriesManager.VerifyCanOpenCloseInventory()) return;
                    _inventoriesManager.OpenInventories();
                }
                break;


            case UIState.Inventories:
                if (InputManager.wantsToInventory)
                {
                    if (!_inventoriesManager.VerifyCanOpenCloseInventory()) return;
                    _inventoriesManager.CloseInventories();
                    currentState = UIState.Nothing;
                }
                break;


            case UIState.HeroesInfos:

                if (InputManager.wantsToHeroInfo)
                {
                    if (!_heroInfosScreen.VerifyCanOpenOrCloseHeroInfos()) return;
                    StartCoroutine(_heroInfosScreen.CloseInfosScreenCoroutine());
                    currentState = UIState.Nothing;
                }
                break;
        }
    }

    public void OpenInventory()
    {
        currentState = UIState.Inventories;
    }
}
