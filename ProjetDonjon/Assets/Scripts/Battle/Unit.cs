using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Unit : MonoBehaviour
{
    protected struct StatModificatorStruct
    {
        public StatModificatorStruct(SkillEffectType type, int addStrength, float multStrength, int duration, Unit origin)
        {
            modificatorType = type;
            modificatorAdditiveStrength = addStrength;
            modificatorMultipliedStrength = multStrength;
            modificatorTurnsLeft = duration;
            modificatorOrigin = origin;
        }

        public SkillEffectType modificatorType;
        public int modificatorAdditiveStrength;
        public float modificatorMultipliedStrength;
        public int modificatorTurnsLeft;
        public Unit modificatorOrigin;
    }

    [Header("Actions")]
    public Action OnHeroInfosChange;

    [Header("Private Stats")]
    private int currentHealth;
    private int currentMaxHealth;
    private int currentStrength;
    private int currentSpeed;
    private int currentLuck;
    private int currentMovePoints;
    private int currentShield;

    [Header("Private Stats Modificators")]
    private int strengthModificatorAdditive;
    private int speedModificatorAdditive;
    private int luckModificatorAdditive;
    private int movePointsModificatorAdditive;
    private List<StatModificatorStruct> currentStatsModificators = new List<StatModificatorStruct>(); 

    [Header("Protected Other Infos")]
    protected BattleTile currentTile;
    protected bool isUnitsTurn;
    protected UnitData unitData;
    protected Unit provocationTarget;

    [Header("Public Infos")]
    public int CurrentHealth { get { return currentHealth; } set { currentHealth = value; _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth); OnHeroInfosChange?.Invoke(); } }
    public int CurrentShield { get { return currentShield; } set { currentShield = value; } }
    public int CurrentMaxHealth { get { return currentMaxHealth; } }
    public int CurrentStrength { get { return currentStrength + strengthModificatorAdditive; } }
    public int CurrentSpeed { get { return currentSpeed + speedModificatorAdditive; } }
    public int CurrentLuck { get { return currentLuck + luckModificatorAdditive; } }
    public int CurrentMovePoints { get { return currentMovePoints + movePointsModificatorAdditive; } }
    public UnitData UnitData { get { return unitData; } }
    public BattleTile CurrentTile { get { return currentTile; } }

    [Header("References")]
    [SerializeField] private UnitUI _ui;
    public Animator _animator;
    public UnitAnimsInfos _unitAnimsInfos;



    #region Unit Infos Functions

    public void InitialiseUnitInfos(int maxHealth, int strength, int speed, int luck, int movePoints)
    {
        currentHealth = maxHealth;
        currentMaxHealth = maxHealth;
        currentStrength = strength;
        currentSpeed = speed;
        currentLuck = luck;

        currentMovePoints = movePoints;

        _ui.ActualiseUI(1, currentHealth);
    }

    public void ActualiseUnitInfos(int newMaxHealth, int newStrength, int newSpeed, int newLuck, int newMovePoints)
    {
        currentMaxHealth = newMaxHealth;
        currentStrength = newStrength;
        currentSpeed = newSpeed;
        currentLuck = newLuck;

        currentMovePoints = newMovePoints;

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth);
    }

    #endregion 


    #region Move Functions

    public IEnumerator MoveUnitCoroutine(BattleTile[] pathTiles)
    {
        for(int i = 0; i < pathTiles.Length; i++)
        {
            MoveUnit(pathTiles[i]);

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void MoveUnit(BattleTile tile)
    {
        if(currentTile != null)
        {
            currentTile.UnitLeaveTile();
        }

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        tile.UnitEnterTile(this);
        currentTile = tile;

        transform.position = tile.transform.position;
    }

    public void PushUnit(Vector2Int direction, int strength)
    {
        List<BattleTile> crossedTiles = new List<BattleTile>();

        for(int i = 0; i < strength; i++)
        {
            Vector2Int currentPos = currentTile.TileCoordinates + direction * (i + 1);

            if (BattleManager.Instance.BattleRoom.PlacedBattleTiles[currentPos.x, currentPos.y] is null) break;
            if (BattleManager.Instance.BattleRoom.PlacedBattleTiles[currentPos.x, currentPos.y].UnitOnTile is not null) break;

            crossedTiles.Add(BattleManager.Instance.BattleRoom.PlacedBattleTiles[currentPos.x, currentPos.y]);
        }

        StartCoroutine(MoveUnitCoroutine(crossedTiles.ToArray()));
    }

    #endregion


    #region Enter / Exit Functions

    public virtual void EnterBattle(BattleTile startTile)
    {
        _ui.ShowUnitUI();
        currentTile = startTile;
    }

    public virtual void ExitBattle(Hero currentHero)
    {
        _ui.HideUnitUI();
    }

    #endregion


    #region Use / Apply Skills

    public void TakeDamage(int damageAmount)
    {
        _animator.SetTrigger("Damage");

        CameraManager.Instance.DoCameraShake(0.25f, Mathf.Lerp(0.2f, 0.5f, damageAmount / 10f), 0.025f);

        int shieldDamages = Mathf.Clamp(damageAmount, 0, CurrentShield);
        int healthDamages = damageAmount - shieldDamages;
    
        CurrentShield -= shieldDamages;
        CurrentHealth -= healthDamages;

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        currentTile.UnitLeaveTile();

        BattleManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
    }


    public void AddStatsModificator(SkillEffectType buffType, int additiveStrength, float multipliedStrength, int duration, Unit origin)
    {
        currentStatsModificators.Add(new StatModificatorStruct(buffType, additiveStrength, multipliedStrength, duration, origin));

        ActualiseBuffsAndDebuffs(false);
    }


    private void ActualiseBuffsAndDebuffs(bool endTurn)
    {
        if (endTurn)
        {
            for (int i = currentStatsModificators.Count - 1; i >= 0; i--)
            {
                StatModificatorStruct current = currentStatsModificators[i];
                current.modificatorTurnsLeft--;
                currentStatsModificators[i] = current;

                if (currentStatsModificators[i].modificatorTurnsLeft <= 0)
                {
                    currentStatsModificators.RemoveAt(i);
                }
            }
        }

        movePointsModificatorAdditive = 0;
        strengthModificatorAdditive = 0;
        speedModificatorAdditive = 0;
        luckModificatorAdditive = 0;
        provocationTarget = null; 

        for(int i = 0; i < currentStatsModificators.Count; i++)
        {
            switch(currentStatsModificators[i].modificatorType)
            {
                case SkillEffectType.ModifyMove:
                    movePointsModificatorAdditive += currentStatsModificators[i].modificatorAdditiveStrength;
                    break;

                case SkillEffectType.ModifyStrength:
                    strengthModificatorAdditive += currentStatsModificators[i].modificatorAdditiveStrength;
                    break;

                case SkillEffectType.ModifyLuck:
                    luckModificatorAdditive += currentStatsModificators[i].modificatorAdditiveStrength;
                    break;

                case SkillEffectType.ModifySpeed:
                    speedModificatorAdditive += currentStatsModificators[i].modificatorAdditiveStrength;
                    break;

                case SkillEffectType.Provoke:
                    provocationTarget = currentStatsModificators[i].modificatorOrigin;
                    break;
            }
        }
    }


    public void AddShield(int addedShield)
    {
        CurrentShield += addedShield;
    }

    #endregion


    public void StartTurn()
    {
        isUnitsTurn = true;
    }

    public void EndTurn()
    {
        isUnitsTurn = false;

        ActualiseBuffsAndDebuffs(true);

        StartCoroutine(BattleManager.Instance.NextTurnCoroutine());
    }
}
