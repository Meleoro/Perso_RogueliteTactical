using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public enum UIState
{
    Nothing,
    Inventories,
    HeroesInfos,
    SkillTrees,
    Skills
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
    public CoinUI CoinUI { get { return _coinUI; } }
    public Minimap Minimap { get { return _minimap; } }
    public FloorTransitionText FloorTransitionText { get { return _floorTransitionText; } }
    public HeroInfosScreen HeroInfosScreen { get { return _heroInfosScreen; } }

    [Header("Actions")]
    public Action OnStartDrag;
    public Action OnStopDrag;

    [Header("Private Infos")]
    private UIState currentState;
    private Loot draggedLoot;

    [Header("References")]
    [SerializeField] private InventoriesManager _inventoriesManager;
    [SerializeField] private HeroInfosScreen _heroInfosScreen;
    [SerializeField] private SkillTreeManager _skillTreesManager;
    [SerializeField] private SkillsMenu _skillsMenu;
    [SerializeField] private AlterationDetailsPanel _alterationDetailsPanel;
    [SerializeField] private Image _transitionFadeImage;
    [SerializeField] private CoinUI _coinUI;
    [SerializeField] private Minimap _minimap;
    [SerializeField] private FloorTransitionText _floorTransitionText;

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

        _skillTreesManager.OnSkillTreeOpen += () => currentState = UIState.SkillTrees;
        _skillTreesManager.OnSkillTreeClose += () => currentState = UIState.Nothing;

        _skillsMenu.OnShow += () => currentState = UIState.Skills;
        _skillsMenu.OnHide += () => currentState = UIState.Nothing;

        _transitionFadeImage.color = new Color(_transitionFadeImage.color.r, _transitionFadeImage.color.g, _transitionFadeImage.color.b, 1);
        StartCoroutine(FloorTransitionText.IntroCoroutine(2f));
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

                if (InputManager.wantsToSkillTree)
                {
                    _skillTreesManager.Show();
                }

                if (InputManager.wantsToSkills)
                {
                    _skillsMenu.Show(); 
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

            case UIState.SkillTrees:
                if (InputManager.wantsToSkillTree)
                {
                    _skillTreesManager.Hide();
                }
                break;

            case UIState.Skills:
                if (InputManager.wantsToSkills)
                {
                    _skillsMenu.Hide();
                }
                break;
        }
    }

    public void OpenInventory()
    {
        currentState = UIState.Inventories;
    }


    public void FadeScreen(float duration, float finalValue)
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
