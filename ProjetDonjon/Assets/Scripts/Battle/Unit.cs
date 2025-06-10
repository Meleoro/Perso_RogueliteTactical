using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;
public struct StatModificatorStruct
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

public class Unit : MonoBehaviour
{
    [Header("Main parameters")]
    [SerializeField] protected bool isEnemy = true;

    [Header("Actions")]
    public Action OnHeroInfosChange;

    [Header("Alteration VFXs")]
    [SerializeField] private GameObject shieldVFX;
    [SerializeField] private GameObject healVFX;

    [Header("Outline Color")]
    [SerializeField] protected Color overlayColor = Color.white;
    [SerializeField] protected Color positiveColor = Color.green;
    [SerializeField] protected Color negativeColor = Color.red;
    [SerializeField] protected Color unitsTurnColor = Color.yellow; 
 
    [Header("Private Infos")]
    private int currentHealth;
    private int currentMaxHealth;
    private int currentStrength;
    private int currentSpeed;
    private int currentLuck;
    private int currentMovePoints;
    private int currentShield;
    private bool restartTurnOutlineNext;

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
    protected Coroutine squishCoroutine;
    protected Coroutine turnOutlineCoroutine;

    [Header("Public Infos")]
    public int CurrentHealth { get { return currentHealth; } set { currentHealth = value; 
            _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentStatsModificators); OnHeroInfosChange?.Invoke(); } }
    public int CurrentShield { get { return currentShield; } set { currentShield = value; } }
    public int CurrentMaxHealth { get { return currentMaxHealth; } }
    public int CurrentStrength { get { return currentStrength + strengthModificatorAdditive; } }
    public int CurrentSpeed { get { return currentSpeed + speedModificatorAdditive; } }
    public int CurrentLuck { get { return currentLuck + luckModificatorAdditive; } }
    public int CurrentMovePoints { get { return currentMovePoints + movePointsModificatorAdditive; } }
    public UnitData UnitData { get { return unitData; } }
    public BattleTile CurrentTile { get { return currentTile; } }
    public bool IsEnemy { get { return isEnemy; } }

    public bool IsHindered { get {
            bool result = false;
            foreach(StatModificatorStruct modificator in currentStatsModificators)
            {
                if(modificator.modificatorType == SkillEffectType.Hinder)
                {
                    result = true;
                }
            }
            return result;
        } }

    [Header("References")]
    [SerializeField] protected UnitUI _ui;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
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

        _ui.HoverAction += HoverUnit;
        _ui.UnHoverAction += UnHoverUnit;
        _ui.ClickAction += ClickUnit;

        _ui.ActualiseUI(1, currentHealth, currentStatsModificators);
    }

    public void ActualiseUnitInfos(int newMaxHealth, int newStrength, int newSpeed, int newLuck, int newMovePoints)
    {
        currentMaxHealth = newMaxHealth;
        currentStrength = newStrength;
        currentSpeed = newSpeed;
        currentLuck = newLuck;

        currentMovePoints = newMovePoints;

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentStatsModificators);
    }

    #endregion 


    #region Move Functions

    public IEnumerator MoveUnitCoroutine(BattleTile[] pathTiles)
    {
        for(int i = 0; i < pathTiles.Length; i++)
        {
            MoveUnit(pathTiles[i], true);

            yield return new WaitForSeconds(0.15f);
        }
    }

    public void MoveUnit(BattleTile tile, bool doSquish = false)
    {
        if(currentTile != null)
        {
            currentTile.UnitLeaveTile();
        }

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        tile.UnitEnterTile(this);
        currentTile = tile;

        transform.position = tile.transform.position;

        if (doSquish)
        {
            if(squishCoroutine != null)
                StopCoroutine(squishCoroutine);

            squishCoroutine = StartCoroutine(SquishCoroutine(0.125f));
        }
    }

    protected IEnumerator SquishCoroutine(float duration)
    {
        transform.UChangeScale(duration * 0.24f, new Vector3(1.25f, 0.85f, 1f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        transform.UChangeScale(duration * 0.48f, new Vector3(0.85f, 1.15f, 1f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        transform.UChangeScale(duration * 0.24f, new Vector3(1f, 1f, 1f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.25f);
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

    protected virtual void Die()
    {
        currentTile.UnitLeaveTile();

        BattleManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
    }


    public virtual void Heal(int healedAmount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + healedAmount, 0, CurrentMaxHealth);

        PlayAlterationVFX(SkillEffectType.Heal);
    }


    public void AddStatsModificator(SkillEffectType buffType, int additiveStrength, float multipliedStrength, int duration, Unit origin)
    {
        bool alreadyApplied = false;

        for(int i = 0; i < currentStatsModificators.Count; i++)
        {
            if (currentStatsModificators[i].modificatorType == buffType && 
                ((additiveStrength > 0 && currentStatsModificators[i].modificatorAdditiveStrength > 0) || 
                (additiveStrength < 0 && currentStatsModificators[i].modificatorAdditiveStrength < 0)))
            {
                alreadyApplied = true;

                StatModificatorStruct current = currentStatsModificators[i];

                current.modificatorTurnsLeft = duration > currentStatsModificators[i].modificatorTurnsLeft ?
                    duration : currentStatsModificators[i].modificatorTurnsLeft;

                current.modificatorAdditiveStrength = additiveStrength > currentStatsModificators[i].modificatorAdditiveStrength ?
                    additiveStrength : currentStatsModificators[i].modificatorAdditiveStrength;

                currentStatsModificators[i] = current;
            }
        }

        if (!alreadyApplied)
        {
            currentStatsModificators.Add(new StatModificatorStruct(buffType, additiveStrength, multipliedStrength, duration, origin));
        }

        PlayAlterationVFX(buffType);
        ActualiseBuffsAndDebuffs(false);
    }


    private void PlayAlterationVFX(SkillEffectType type)
    {
        switch (type)
        {
            case SkillEffectType.AddShield:
                Destroy(Instantiate(shieldVFX, transform.position, Quaternion.Euler(0, 0, 0)), 2f);
                break;

            case SkillEffectType.Heal:
                Destroy(Instantiate(healVFX, transform.position, Quaternion.Euler(0, 0, 0)), 2f);
                break;
        }
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

                case SkillEffectType.AddShield:
                    CurrentShield = currentStatsModificators[i].modificatorAdditiveStrength;
                    break;
            }
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentStatsModificators);
    }

    #endregion


    #region Mouse Inputs

    private void HoverUnit()
    {
        CurrentTile?.OverlayTile();
    }

    private void UnHoverUnit()
    {
        CurrentTile?.QuitOverlayTile();
    }

    protected virtual async void ClickUnit()
    {
        await Task.Yield(); 

        if (InputManager.wantsToRightClick) return;

        StartCoroutine(SquishCoroutine(0.15f));
        CurrentTile?.ClickTile();
    }

    #endregion


    #region Outline Functions

    public void HideOutline()
    {
        if (restartTurnOutlineNext)
        {
            restartTurnOutlineNext = false;
            StartTurnOutline();
        }

        _spriteRenderer.material.ULerpMaterialColor(0.15f, new Color(0, 0, 0, 0), "_OutlineColor");
    }

    protected void DisplayOutline(Color color)
    {
        if(turnOutlineCoroutine != null)
        {
            restartTurnOutlineNext = true;
            EndTurnOutline();
        }

        _spriteRenderer.material.ULerpMaterialColor(0.15f, color, "_OutlineColor");
    }

    public void DisplayOverlayOutline()
    {
        DisplayOutline(overlayColor);
    }

    public void DisplaySkillOutline(bool isPositive)
    {
        if (isPositive)
        {
            DisplayOutline(positiveColor);
        }
        else
        {
            DisplayOutline(negativeColor);
        }
    }

    protected void StartTurnOutline()
    {
        if(turnOutlineCoroutine != null)
        {
            StopCoroutine(turnOutlineCoroutine);
        }

        if (!isUnitsTurn) return;

        turnOutlineCoroutine = StartCoroutine(TurnOutlineCoroutine());
    }

    protected void EndTurnOutline()
    {
        if (turnOutlineCoroutine != null)
        {
            StopCoroutine(turnOutlineCoroutine);
        }
    }

    private IEnumerator TurnOutlineCoroutine()
    {
        while (true)
        {
            _spriteRenderer.material.ULerpMaterialColor(0.7f, unitsTurnColor, "_OutlineColor");

            yield return new WaitForSeconds(0.9f);

            _spriteRenderer.material.ULerpMaterialColor(0.7f, new Color(0, 0, 0, 0), "_OutlineColor");

            yield return new WaitForSeconds(0.9f);
        }
    }

    #endregion


    public virtual void StartTurn()
    {
        isUnitsTurn = true;

        StartTurnOutline();
    }

    public virtual void EndTurn(float delay)
    {
        isUnitsTurn = false;
        restartTurnOutlineNext = false;

        EndTurnOutline();
        HideOutline();

        ActualiseBuffsAndDebuffs(true);

        StartCoroutine(BattleManager.Instance.NextTurnCoroutine(delay));
    }
}
