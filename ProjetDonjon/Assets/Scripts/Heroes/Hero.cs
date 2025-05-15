using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Unit
    {
    [Header("Parameters")]
    [SerializeField] private HeroData heroData;

    [Header("Public Infos")]
    public Loot[] EquippedLoot { get { return equippedLoot; } }
    public Inventory Inventory { get { return inventory; } }
    public HeroData HeroData { get { return heroData; } }
    public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; OnHeroInfosChange?.Invoke(); } }  
    public int CurrentSkillPoints { get { return currentSkillPoints; } set { currentSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentMaxSkillPoints { get { return currentMaxSkillPoints; } set { currentMaxSkillPoints = value; OnHeroInfosChange?.Invoke(); } }

    [Header("Private Infos")]
    private Loot[] equippedLoot = new Loot[6];
    private Inventory inventory;
    private bool isHidden;
    private Transform currentDisplayedHeroTr;
    private int currentLevel;
    private int currentSkillPoints;
    private int currentMaxSkillPoints;

    [Header("References")]
    [SerializeField] private HeroController _controller;
    [SerializeField] private Transform _spriteRendererParent;
    [SerializeField] private HeroInfoPanel _heroInfoPanel;


    public void Start()
    {
        unitData = heroData;
        _controller.EndAutoMoveAction += HideHero;

        InitialiseUnitInfos(heroData.baseHealth, heroData.baseStrength, heroData.baseSpeed, heroData.baseLuck, heroData.baseMovePoints);
        CurrentSkillPoints = HeroData.startSkillPoints;
        CurrentMaxSkillPoints = HeroData.maxSkillPoints;
    }

    private void Update()
    {
        if (isHidden)
        {
            transform.position = currentDisplayedHeroTr.transform.position;
        }
        else
        {
            _controller.UpdateController();
        }
    }


    #region Hide / Show Functions

    public void ShowHero()
    {
        isHidden = false;

        _spriteRendererParent.gameObject.SetActive(true);
    }

    public void HideHero()
    {
        isHidden = true;

        _spriteRendererParent.gameObject.SetActive(false);
    }

    public void ActualiseCurrentDisplayedHero(Transform heroTr)
    {
        currentDisplayedHeroTr = heroTr;
    }

    #endregion


    #region Battle Functions

    public override void EnterBattle(BattleTile startTile)
    {
        base.EnterBattle(startTile);
        _controller.EnterBattle();
        StartCoroutine(_controller.AutoMoveCoroutine(startTile.transform.position));
    }

    public override void ExitBattle(Hero currentHero)
    {
        base.ExitBattle(currentHero);
        _controller.ExitBattle();

        if (currentHero == this) return;
        StartCoroutine(_controller.AutoMoveCoroutineEndBattle(currentHero.transform));
    }

    #endregion


    #region Inventory Functions

    public void SetupInventory(Inventory inventory)
    {
        this.inventory = inventory;
    }

    public void AddEquipment(Loot loot, int equipSlotIndex)
    {
        equippedLoot[equipSlotIndex] = loot;
    }

    public void RemoveEquipment(Loot removedLoot, int unequipSlotIndex)
    {
        equippedLoot[unequipSlotIndex] = null;
    }

    #endregion


    #region Others

    public void SetupHeroInfosPanel(HeroInfoPanel heroInfoPanel)
    {
        _heroInfoPanel = heroInfoPanel;
        _heroInfoPanel.InitialisePanel(this);

        OnHeroInfosChange += ActualiseHeroInfoPanel;
    }

    private void ActualiseHeroInfoPanel()
    {
        _heroInfoPanel.ActualisePanel(this);
    }

    #endregion
}
