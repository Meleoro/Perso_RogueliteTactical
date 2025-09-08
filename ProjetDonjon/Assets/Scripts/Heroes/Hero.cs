using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hero : Unit
{
    [Header("Hero Parameters")]
    [SerializeField] private HeroData heroData;
    [SerializeField] private SkillTreeData skillTreeData;
    [SerializeField] private int heroIndex;        // ID To access heroes save
    public LootData[] startLoot;

    [Header("Actions")]
    public Action OnClickUnit;
    public Action OnDead;
    public Action OnRevive;

    [Header("Public Infos")]
    public Loot[] EquippedLoot { get { return equippedLoot; } }
    public SkillData[] EquippedSkills { get { return equippedSkills; } }
    public PassiveData[] EquippedPassives { get { return equippedPassives; } }
    public Inventory Inventory { get { return inventory; } }
    public HeroData HeroData { get { return heroData; } }
    public SkillTreeData SkillTreeData { get { return skillTreeData; } }
    public HeroController Controller { get { return _controller; } }
    public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; OnHeroInfosChange?.Invoke(); } }  
    public int CurrentSkillPoints { get { return currentSkillPoints; } set { currentSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentMaxSkillPoints { get { return currentMaxSkillPoints; } set { currentMaxSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentActionPoints { get { return currentActionPoints; } }
    public int CurrentSkillTreePoints { get { return currentSkillTreePoints; } }
    public bool[] SkillTreeUnlockedNodes { get { return skillTreeUnlockedNodes; } }
    public int HeroIndex { get { return heroIndex; } }
    public UnitUI UI { get { return _ui; } }


    [Header("Private Infos")]
    private Loot[] equippedLoot = new Loot[6];
    private SkillData[] equippedSkills = new SkillData[6];
    private PassiveData[] equippedPassives = new PassiveData[3];
    private Inventory inventory;
    private bool isHidden;
    private bool isDead;
    private Transform currentDisplayedHeroTr;
    private int currentSkillPoints;
    private int currentMaxSkillPoints;
    private int currentActionPoints;
    private int currentXP;
    private int XPToReach;
    private int currentLevel;
    private int currentSkillTreePoints;
    private bool[] skillTreeUnlockedNodes;


    [Header("Hero References")]
    [SerializeField] private HeroController _controller;
    [SerializeField] private Transform _spriteRendererParent;
    [SerializeField] private HeroInfoPanel _heroInfoPanel;
    [SerializeField] private Collider2D[] _colliders;


    public void Start()
    {
        unitData = heroData;
        _controller.EndAutoMoveAction += HideHero;

        skillTreeUnlockedNodes = new bool[15];
        for(int i = 0; i < 15; i++)
        {
            if(HeroesManager.Instance.unlockAllSkills) skillTreeUnlockedNodes[i] = true;
            else skillTreeUnlockedNodes[i] = HeroesManager.Instance.HeroesUnlockedNodes[heroIndex * 15 + i];
        }
        currentSkillTreePoints = HeroesManager.Instance.HeroesCurrentSkillTreePoint[heroIndex];

        InitialiseUnitInfos(heroData.baseHealth, heroData.baseStrength, heroData.baseSpeed, heroData.baseLuck, heroData.baseMovePoints);
        CurrentSkillPoints = HeroData.startSkillPoints;
        CurrentMaxSkillPoints = HeroData.maxSkillPoints;

        currentLevel = HeroesManager.Instance.HeroesLevel[heroIndex];
        XPToReach = (int)(10 * (currentLevel * 1.25f));
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            GainLevel();
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

        if(CurrentHealth <= 0)
        {
            _animator.SetBool("IsDead", true);
        }
    }

    public override void ExitBattle(Hero currentHero)
    {
        base.ExitBattle(currentHero);
        _controller.ExitBattle();

        RemoveAllAlterations();

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

        if(BattleManager.Instance.IsInBattle)
            BattleManager.Instance.RemoveUnit(this);

        OnDead.Invoke();

        isDead = true;
    }

    public override void Heal(int healedAmount)
    {
        base.Heal(healedAmount);

        if (isDead)
        {
            OnRevive.Invoke();

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


    #region Level Functions

    public void GainXP(int quantity)
    {
        currentXP += quantity;
        _ui.GainXP(Mathf.Clamp((float)currentXP / XPToReach, 0f, 1f));

        if (currentXP >= XPToReach) StartCoroutine(GainLevelCoroutine(0.5f));
    }
    
    private IEnumerator GainLevelCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        StartCoroutine(_ui.DoLevelUpEffectCoroutine(1f));

        currentLevel++;
        currentSkillTreePoints++;

        currentXP = currentXP - XPToReach;
        XPToReach = (int)(XPToReach * 1.25f);

        _ui.ResetXPProgress();

        yield return new WaitForSeconds(0.25f);

        _ui.GainXP((float)currentXP / XPToReach);
    }

    private void GainLevel()
    {
        currentLevel++;
        currentSkillTreePoints++;

        currentXP = currentXP - XPToReach;
        XPToReach = (int)(XPToReach * 1.25f);
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

    // Called when we buy a new skill to actualise hero infos
    public void ActualiseSkillTreeUnlockedNodes(bool[] unlockedNodes)
    {
        skillTreeUnlockedNodes = unlockedNodes;

        currentSkillTreePoints--;
    }

    // Called when we modify the equipped skills
    public void ActualiseEquippedSkills(SkillData[] equippedSkills)
    {
        this.equippedSkills = equippedSkills;
    }

    // Called when we modify the equipped passives
    public void ActualiseEquippedPassives(PassiveData[] equippedPassives)
    {
        this.equippedPassives = equippedPassives;
    }


    public void LoadEquippedInfos(string[] allSavedSkills, string[] allSavedPasssives)
    {
        string[] heroSavedSkills = new string[6];
        string[] heroSavedPassives = new string[3];

        for(int i = heroIndex * 6;  i < heroIndex * 6 + 6; i++)
        {
            heroSavedSkills[i - heroIndex * 6] = allSavedSkills[i];
        }

        for (int i = heroIndex * 3; i < heroIndex * 3 + 3; i++)
        {
            heroSavedPassives[i - heroIndex * 3] = allSavedPasssives[i];
        }

        LoadEquippedSkills(heroSavedSkills);
        LoadEquippedPassives(heroSavedPassives);
    }


    private void LoadEquippedSkills(string[] skillsNames)
    {
        for(int i = 0; i < skillsNames.Length; i++)
        {
            for(int j = 0; j < heroData.heroSkillPool.Length; j++)
            {
                if (heroData.heroSkillPool[j].skillName != skillsNames[i]) continue;

                equippedSkills[i] = heroData.heroSkillPool[j];  
                break;
            }
        }
    }

    private void LoadEquippedPassives(string[] passivesNames)
    {
        for (int i = 0; i < passivesNames.Length; i++)
        {
            for (int j = 0; j < heroData.heroPassivePool.Length; j++)
            {
                if (heroData.heroPassivePool[j].passiveName != passivesNames[i]) continue;

                equippedPassives[i] = heroData.heroPassivePool[j];
                break;
            }
        }
    }

    #endregion
}
