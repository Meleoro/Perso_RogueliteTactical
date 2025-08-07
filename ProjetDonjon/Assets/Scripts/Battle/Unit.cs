using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;


public struct AlterationStruct
{
    public AlterationStruct(AlterationData alteration, int duration, float strength, Unit origin)
    {
        this.alteration = alteration;
        this.duration = duration;
        this.currentStrength = strength;
        this.origin = origin;
    }

    public AlterationData alteration;
    public int duration;
    public float currentStrength;
    public Unit origin;
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
    private float critModificatorAdditive;
    private List<AlterationStruct> currentAlterations = new List<AlterationStruct>(); 

    [Header("Protected Other Infos")]
    protected BattleTile currentTile;
    protected bool isUnitsTurn;
    protected UnitData unitData;
    protected Unit provocationTarget;
    protected Coroutine squishCoroutine;
    protected Coroutine turnOutlineCoroutine;

    [Header("Public Infos")]
    public int CurrentHealth { get { return currentHealth; } set { currentHealth = value; 
            _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations); OnHeroInfosChange?.Invoke(); } }
    public int CurrentShield { get { return currentShield; } set { currentShield = value; } }
    public int CurrentMaxHealth { get { return currentMaxHealth; } }
    public int CurrentStrength { get { return currentStrength + strengthModificatorAdditive; } }
    public int CurrentSpeed { get { return currentSpeed + speedModificatorAdditive; } }
    public int CurrentLuck { get { return currentLuck + luckModificatorAdditive; } }
    public int CurrentMovePoints { get { return currentMovePoints + movePointsModificatorAdditive; } }
    public float CurrentCritMultiplier { get { return 2 + critModificatorAdditive; } }
    public UnitData UnitData { get { return unitData; } }
    public BattleTile CurrentTile { get { return currentTile; } }
    public bool IsEnemy { get { return isEnemy; } }

    public bool IsHindered { get {
            bool result = false;
            foreach(AlterationStruct altStruct in currentAlterations)
            {
                if(altStruct.alteration.alterationType == AlterationType.Hindered)
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

        _ui.ActualiseUI(1, currentHealth, currentAlterations);

        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
    }

    public void ActualiseUnitInfos(int newMaxHealth, int newStrength, int newSpeed, int newLuck, int newMovePoints)
    {
        int currentDamages = currentMaxHealth - currentHealth;

        currentMaxHealth = newMaxHealth;
        currentHealth = currentMaxHealth - currentDamages;
        currentStrength = newStrength;
        currentSpeed = newSpeed;
        currentLuck = newLuck;

        currentMovePoints = newMovePoints;

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
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

        BattleManager.Instance._pathCalculator.ActualisePathCalculatorTiles(BattleManager.Instance.BattleRoom.PlacedBattleTiles);
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

    public bool VerifyCrit(Unit[] attackedUnits)
    {
        int averageLuck = 0;
        for(int i = 0; i < attackedUnits.Length; i++)
        {
            averageLuck += attackedUnits[i].CurrentLuck;
        }
        averageLuck /= attackedUnits.Length;

        int critProba = ((CurrentLuck - averageLuck) + 3) * 3;
        int pickedProba = Random.Range(0, 100);

        return pickedProba < critProba;
    }

    public virtual void TakeDamage(int damageAmount, Unit originUnit)
    {
        _animator.SetTrigger("Damage");
        CameraManager.Instance.DoCameraShake(0.25f, Mathf.Lerp(0.2f, 0.5f, damageAmount / 10f), 0.025f);

        // Thorn
        if (originUnit)
        {
            AlterationData thornAlt = VerifyHasAlteration(AlterationType.Thorn);
            if (thornAlt) originUnit.TakeDamage((int)thornAlt.strength, null);
        }

        // Weakened
        AlterationData weakenedAlt = VerifyHasAlteration(AlterationType.Vulnerable);
        if (weakenedAlt) damageAmount = (int)(damageAmount * weakenedAlt.strength);

        int shieldDamages = Mathf.Clamp(damageAmount, 0, CurrentShield);
        int healthDamages = damageAmount - shieldDamages;

        if (shieldDamages == CurrentShield && CurrentShield > 0) RemoveAlteration(AlterationType.Shield);

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
        if (healedAmount != -1)
            CurrentHealth = Mathf.Clamp(CurrentHealth + healedAmount, 0, CurrentMaxHealth);
        else
            CurrentHealth = CurrentMaxHealth;
    }

    #endregion


    #region Alterations

    public void AddAlteration(AlterationData alteration, Unit origin)
    {
        bool alreadyApplied = false;

        for (int i = 0; i < currentAlterations.Count; i++)
        {
            if (currentAlterations[i].alteration.alterationType == alteration.alterationType)
            {
                alreadyApplied = true;

                AlterationStruct current = currentAlterations[i];

                current.duration = alteration.duration > currentAlterations[i].duration ?
                    alteration.duration : currentAlterations[i].duration;

                if (alteration.isStackable)
                {
                    current.currentStrength += alteration.strength;
                }

                currentAlterations[i] = current;
            }
        }

        if (!alreadyApplied)
        {
            currentAlterations.Add(new AlterationStruct(alteration, alteration.duration, alteration.strength, origin));
        }

        PlayAlterationVFX(alteration.alterationType);
        ActualiseBuffsAndDebuffs(false);
    }


    public AlterationData VerifyHasAlteration(AlterationType type)
    {
        foreach (AlterationStruct altStruct in currentAlterations)
        {
            if (altStruct.alteration.alterationType == type)
            {
                return altStruct.alteration;
            }
        }
        return null;
    }


    private void PlayAlterationVFX(AlterationType type)
    {
        switch (type)
        {
            case AlterationType.Shield:
                Destroy(Instantiate(shieldVFX, transform.position, Quaternion.Euler(0, 0, 0)), 2f);
                break;
        }
    }


    private void RemoveAlteration(AlterationType type)
    {
        for (int i = currentAlterations.Count - 1; i >= 0; i--)
        {
            if (currentAlterations[i].alteration.alterationType == type)
            {
                currentAlterations.RemoveAt(i);
                break;
            }
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }


    private void ActualiseBuffsAndDebuffs(bool endTurn)
    {
        if (endTurn)
        {
            for (int i = currentAlterations.Count - 1; i >= 0; i--)
            {
                AlterationStruct current = currentAlterations[i];

                if (!current.alteration.isInfinite)
                    current.duration--;

                currentAlterations[i] = current;

                if (currentAlterations[i].duration <= 0 && !currentAlterations[i].alteration.isInfinite)
                {
                    currentAlterations.RemoveAt(i);
                }
            }
        }

        movePointsModificatorAdditive = 0;
        strengthModificatorAdditive = 0;
        speedModificatorAdditive = 0;
        luckModificatorAdditive = 0;
        currentShield = 0;
        provocationTarget = null;

        for (int i = 0; i < currentAlterations.Count; i++)
        {
            switch (currentAlterations[i].alteration.alterationType)
            {
                case AlterationType.Strength:
                    strengthModificatorAdditive += (int)(currentStrength * currentAlterations[i].currentStrength);
                    break;

                case AlterationType.Weakened:
                    strengthModificatorAdditive -= (int)(currentStrength * currentAlterations[i].currentStrength);
                    break;

                case AlterationType.Provocked:
                    provocationTarget = currentAlterations[i].origin;
                    break;

                case AlterationType.Shield:
                    CurrentShield += (int)currentAlterations[i].currentStrength;
                    break;

                case AlterationType.Lucky:
                    luckModificatorAdditive += (int)(currentLuck * currentAlterations[i].currentStrength);
                    break;

                case AlterationType.Unlucky:
                    luckModificatorAdditive -= (int)(currentLuck * currentAlterations[i].currentStrength);
                    break;
            }
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
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
        await Task.Delay((int)(Time.deltaTime * 1000));

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


    public void UseItem(LootData itemData)
    {
        switch (itemData.consumableType)
        {
            case ConsumableType.Heal:
                Heal(itemData.consumablePower);
                break;

            case ConsumableType.Focus:
                (this as Hero).AddSkillPoints(itemData.consumablePower);
                break;

            case ConsumableType.Action:
                (this as Hero).AddActionPoints(itemData.consumablePower);
                break;
        }
    }


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
