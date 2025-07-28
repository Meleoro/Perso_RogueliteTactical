using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hero : Unit
    {
    [Header("Hero Parameters")]
    [SerializeField] private HeroData heroData;
    public LootData[] startLoot;

    [Header("Actions")]
    public Action OnClickUnit;

    [Header("Public Infos")]
    public Loot[] EquippedLoot { get { return equippedLoot; } }
    public Inventory Inventory { get { return inventory; } }
    public HeroData HeroData { get { return heroData; } }
    public HeroController Controller { get { return _controller; } }
    public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; OnHeroInfosChange?.Invoke(); } }  
    public int CurrentSkillPoints { get { return currentSkillPoints; } set { currentSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentMaxSkillPoints { get { return currentMaxSkillPoints; } set { currentMaxSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentActionPoints { get { return currentActionPoints; } }

    [Header("Private Infos")]
    private Loot[] equippedLoot = new Loot[6];
    private Inventory inventory;
    private bool isHidden;
    private bool isDead;
    private Transform currentDisplayedHeroTr;
    private int currentLevel;
    protected int currentSkillPoints;
    private int currentMaxSkillPoints;
    private int currentActionPoints;

    [Header("Hero References")]
    [SerializeField] private HeroController _controller;
    [SerializeField] private Transform _spriteRendererParent;
    [SerializeField] private HeroInfoPanel _heroInfoPanel;
    [SerializeField] private Collider2D[] _colliders;


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

    public void ShowHero(bool isFromHeroSwitch = false)
    {
        isHidden = false;

        _spriteRendererParent.gameObject.SetActive(true);

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i].isTrigger || isFromHeroSwitch)
                _colliders[i].enabled = true;
        }
    }

    public void HideHero()
    {
        isHidden = true;

        _spriteRendererParent.gameObject.SetActive(false);

        for(int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i].enabled = false;
        }
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
        _controller.AutoMove(startTile.transform.position);

        foreach(Loot equipment in equippedLoot)
        {
            if (equipment is null) continue;
            if (equipment.LootData.equipmentEffectType != SpecialEquipmentEffectType.Alteration) continue;

            AddAlteration(equipment.LootData.equipmentEffectAlteration, this);
        }
    }

    public override void ExitBattle(Hero currentHero)
    {
        base.ExitBattle(currentHero);
        _controller.ExitBattle();

        if (currentHero == this) return;
        StartCoroutine(_controller.AutoMoveCoroutineEndBattle(currentHero.transform));
    }

    public override void StartTurn()
    {
        base.StartTurn();

        if (currentSkillPoints < currentMaxSkillPoints)
        {
            CurrentSkillPoints += 1;
        }
        currentActionPoints = 2;
    }

    public void DoAction()
    {
        currentActionPoints--;
    }

    public override void TakeDamage(int damageAmount, Unit originUnit)
    {
        base.TakeDamage(damageAmount, originUnit);

        LootData[] hitLootEffects = GetEquippedLootOfEffectType(SpecialEquipmentEffectType.HitAlteration);

        for(int i = 0; i < hitLootEffects.Length; i++)
        {
            if (hitLootEffects[i].equipmentEffectAlteration.isPositive)
            {
                int pickedProba = Random.Range(0, 100);
                if (pickedProba > hitLootEffects[i].equipmentEffectPower) continue;

                AddAlteration(hitLootEffects[i].equipmentEffectAlteration, this);
            }
            else
            {
                int pickedProba = Random.Range(0, 100);
                if (pickedProba > hitLootEffects[i].equipmentEffectPower) continue;

                originUnit.AddAlteration(hitLootEffects[i].equipmentEffectAlteration, this);
            }
        }
    }

    protected override void Die()
    {
        _animator.SetBool("IsDead", true);
        BattleManager.Instance.RemoveUnit(this);

        isDead = true;
    }

    public override void Heal(int healedAmount)
    {
        base.Heal(healedAmount);

        if (isDead)
        {
            isDead = false;
            BattleManager.Instance.AddUnit(this);
        }
    }

    protected override void ClickUnit()
    {
        base.ClickUnit();

        OnClickUnit?.Invoke();
    }

    public override void EndTurn(float delay)
    {
        base.EndTurn(delay);
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

    public LootData[] GetEquippedLootOfEffectType(SpecialEquipmentEffectType effectType)
    {
        List<LootData> result = new List<LootData>();

        for(int i = 0; i < 6; i++)
        {
            if (equippedLoot[i] is null) continue;
            if (equippedLoot[i].LootData.equipmentEffectType != effectType) continue;

            result.Add(equippedLoot[i].LootData);
        }

        return result.ToArray();
    }

    #endregion


    #region Others

    public void AddSkillPoints(int quantity)
    {
        if (quantity != -1)
            CurrentSkillPoints = Mathf.Clamp(CurrentSkillPoints + quantity, 0, currentMaxSkillPoints);
        else
            CurrentSkillPoints = currentMaxSkillPoints;


    }


    public void UseSkillPoints(int quantity)
    {
        CurrentSkillPoints = Mathf.Clamp(CurrentSkillPoints - quantity, 0, currentMaxSkillPoints);


    }


    public void AddActionPoints(int quantity)
    {
        currentActionPoints += quantity;


    }

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
