using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
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
    public AlterationDetailsPanel AlterationDetailsPanel { get { return _alterationDetailsPanel; } }

    [Header("Actions")]
    public Action OnStartDrag;
    public Action OnStopDrag;

    [Header("Private Infos")]
    private UIState currentState;
    private Loot draggedLoot;

    [Header("References")]
    [SerializeField] private InventoriesManager _inventoriesManager;
    [SerializeField] private HeroInfosScreen _heroInfosScreen;
    [SerializeField] private AlterationDetailsPanel _alterationDetailsPanel;
    [SerializeField] private Image _transitionFadeImage;

    [Header("References Hero Infos")]
    [SerializeField] private HeroInfoPanel[] _heroInfoPanels1H;
    [SerializeField] private Animator _heroInfoPanelsAnimator1H;
    [SerializeField] private HeroInfoPanel[] _heroInfoPanels2H;
    [SerializeField] private Animator _heroInfoPanelsAnimator2H;
    [SerializeField] private HeroInfoPanel[] _heroInfoPanels3H;
    [SerializeField] private Animator _heroInfoPanelsAnimator3H;


    private void Start()
    {
        _inventoriesManager.OnInventoryOpen += OpenInventory;

        _heroInfosScreen.Open += () => currentState = UIState.HeroesInfos;
        _heroInfosScreen.Close += () => currentState = UIState.Nothing;
    }

    private void Update()
    {
        if (BattleManager.Instance.IsInBattle) return;

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


    public void FadeScreen(int duration, int finalValue)
    {
        _transitionFadeImage.UFadeImage(duration, finalValue);
    }


    #region Battle UI

    public void SetupHeroInfosPanel(Hero[] heroes)
    {
        HeroInfoPanel[] currentPanels = new HeroInfoPanel[0];
        switch (heroes.Length)
        {
            case 1:
                currentPanels = _heroInfoPanels1H;
                break;

            case 2:
                currentPanels = _heroInfoPanels2H;  
                break;

            case 3:
                currentPanels = _heroInfoPanels3H;
                break;
        }

        for (int i = 0; i < heroes.Length; i++)
        {
            currentPanels[i].InitialisePanel(heroes[i]);
            heroes[i].SetupHeroInfosPanel(currentPanels[i]);
        }
    }

    public void ShowHeroInfosPanels()
    {
        switch (HeroesManager.Instance.Heroes.Length)
        {
            case 1:
                _heroInfoPanelsAnimator1H.SetBool("IsOpened", true);
                break;

            case 2:
                _heroInfoPanelsAnimator2H.SetBool("IsOpened", true);
                break;

            case 3:
                _heroInfoPanelsAnimator3H.SetBool("IsOpened", true);
                break;
        }
    }

    public void HideHeroInfosPanels()
    {
        switch (HeroesManager.Instance.Heroes.Length)
        {
            case 1:
                _heroInfoPanelsAnimator1H.SetBool("IsOpened", false);
                break;

            case 2:
                _heroInfoPanelsAnimator2H.SetBool("IsOpened", false);
                break;

            case 3:
                _heroInfoPanelsAnimator3H.SetBool("IsOpened", false);
                break;
        }
    }

    #endregion
}
