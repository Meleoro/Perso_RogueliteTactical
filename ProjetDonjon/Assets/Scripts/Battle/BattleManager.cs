using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;
using static Enums;

public class BattleManager : GenericSingletonClass<BattleManager>
{
    [Header("Parameters")]
    [SerializeField] private Tile[] holeTiles;

    [Header("Actions")]
    public Action OnMoveUnit;
    public Action OnSkillUsed;
    public Action OnBattleEnd;
    public Action OnBattleStart;

    [Header("Private Infos")]
    private bool isInBattle;
    private bool isInBattleCutscene;
    private List<Unit> currentHeroes = new();
    private List<Unit> deadHeroes = new();
    private List<AIUnit> currentEnemies = new();
    private List<AIUnit> currentAllies = new();
    private List<Unit> currentUnits = new();
    private Room battleRoom;
    private Unit currentUnit;
    private SkillData currentSkill;
    private BattleTile currentSkillBaseTile;
    private int currentVFXIndex;

    [Header("Public Infos")]
    public Room BattleRoom { get {  return battleRoom; } }
    public Unit CurrentUnit { get { return currentUnit; } }
    public List<Unit> CurrentHeroes { get { return currentHeroes; } }
    public List<AIUnit> CurrentEnemies { get { return currentEnemies; } }
    public bool IsInBattle {  get { return isInBattle; } }
    public MenuType CurrentActionType { get { return _playerActionsMenu.CurrentMenu; } }
    public Tile[] HoleTiles { get { return holeTiles; } }
    public PathCalculator PathCalculator { get { return _pathCalculator; } }
    public TilesManager TilesManager {  get { return _tilesManager; } }
    public PassivesManager PassivesManager {  get { return _passivesManager; } }


    [Header("References")]
    [SerializeField] private Timeline _timeline;
    [SerializeField] private PlayerActionsMenu _playerActionsMenu;
    private PathCalculator _pathCalculator;
    private TilesManager _tilesManager;
    private PassivesManager _passivesManager;



    private void Start()
    {
        _pathCalculator = new PathCalculator();
        _tilesManager = new TilesManager();
        _passivesManager = new PassivesManager();

        _tilesManager.Initialise(_pathCalculator, _playerActionsMenu);
    }


    private void Update()
    {
        if (!isInBattle) return;
    }


    #region Add / Remove Units

    public void AddUnit(Unit unit)
    {
        currentUnits.Add(unit);

        if (unit.GetType() == typeof(Hero))
        {
            currentHeroes.Add(unit);

            if(deadHeroes.Contains(unit))
                deadHeroes.Remove(unit);
        }
        else
        {
            if((unit as AIUnit).IsEnemy)
            {
                currentEnemies.Add((AIUnit)unit);
            }
            else
            {
                currentHeroes.Add(unit);
                currentAllies.Add((AIUnit)unit);
            }
        }
    }

    public void RemoveUnit(Unit unit)
    {
        _timeline.RemoveUnit(unit);
        currentUnits.Remove(unit);

        if (unit.GetType() == typeof(Hero))
        {
            currentHeroes.Remove((Hero)unit);
            deadHeroes.Add(unit);
        }
        else 
        {
            if ((unit as AIUnit).IsEnemy)
            {
                currentEnemies.Remove((AIUnit)unit);
            }
            else
            {
                currentHeroes.Remove(unit);
                currentAllies.Remove((AIUnit)unit);
            }

            if (currentEnemies.Count == 0 || ((AIUnit)unit).IsBoss)
            {
                EndBattle();
            }
        }

        if(currentHeroes.Count == 0)
        {
            //GameOver
        }
    }

    #endregion


    #region Start / End Battle

    public void StartBattle(List<BattleTile> possibleTiles, Vector3 battleCenterPos, float cameraSize, Room battleRoom, float delay = 0)
    {
        currentUnits.Clear();
        currentHeroes.Clear();
        currentEnemies.Clear();
        currentAllies.Clear();
        _pathCalculator.InitialisePathCalculator(battleRoom.PlacedBattleTiles);

        OnBattleStart.Invoke();

        isInBattle = true;
        this.battleRoom = battleRoom;
        _tilesManager.battleRoom = battleRoom;

        for(int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
        {
            if (HeroesManager.Instance.Heroes[i].CurrentHealth <= 0) continue;
            AddUnit(HeroesManager.Instance.Heroes[i]);
        }
        for (int i = 0; i < battleRoom.RoomEnemies.Count; i++)
        {
            AddUnit(battleRoom.RoomEnemies[i]);
            battleRoom.RoomEnemies[i].EnterBattle(battleRoom.RoomEnemies[i].CurrentTile);
        }

        CameraManager.Instance.EnterBattle(battleCenterPos, cameraSize);
        HeroesManager.Instance.EnterBattle(possibleTiles);

        UIManager.Instance.ShowHeroInfosPanels();

        StartCoroutine(StartBattleCoroutine(delay));
    }

    private IEnumerator StartBattleCoroutine(float delay)
    {
        yield return new WaitForSeconds(2.5f + delay);

        _timeline.InitialiseTimeline(currentUnits);

        StartCoroutine(NextTurnCoroutine(0, false));
    }

    private void EndBattle()
    {
        isInBattleCutscene = false;

        battleRoom.EndBattle();
        OnBattleEnd.Invoke();

        for (int i = deadHeroes.Count() - 1; i >= 0; i--)
        {
            //deadHeroes[i].HideOutline();
            //deadHeroes[i].Heal(1);
        }
        deadHeroes.Clear();

        for (int i = 0; i < currentAllies.Count; i++)
        {
            currentAllies[i].TakeDamage(1000, null);
        }

        CameraManager.Instance.ExitBattle();
        HeroesManager.Instance.ExitBattle();

        _timeline.Disappear();
    }

    public void StartBattleEndCutscene()
    {
        isInBattleCutscene = true;
        isInBattle = false;

        UIManager.Instance.HideHeroInfosPanels();
        _playerActionsMenu.CloseActionsMenu();
    }

    #endregion


    #region Battle Events

    public IEnumerator NextTurnCoroutine(float delay = 0, bool endTurn = true)
    {
        yield return new WaitForSeconds(delay);

        if (!isInBattle) yield break;

        if(endTurn)
            _timeline.NextTurn();

        currentUnit = _timeline.Slots[0].Unit;
        _tilesManager.currentUnit = currentUnit;

        currentUnit.StartTurn();

        if (currentUnit.GetType() == typeof(Hero))
        {
            _playerActionsMenu.SetupHeroActionsUI(currentUnit as Hero);
        }
        else
        {
            CameraManager.Instance.FocusOnTr(currentUnit.transform, 5f);
            AIUnit enemy = currentUnit as AIUnit;
            StartCoroutine(enemy.PlayEnemyTurnCoroutine());
        }
    }


    public IEnumerator MoveUnitCoroutine(BattleTile aimedTile, bool useDiagonals) 
    {
        _pathCalculator.ActualisePathCalculatorTiles(battleRoom.PlacedBattleTiles);

        if(currentUnit.CurrentTile.TileCoordinates == aimedTile.TileCoordinates)
        {
            _tilesManager.ResetTiles();

            if (CurrentUnit.GetType() == typeof(Hero))
                OnMoveUnit.Invoke();

            yield break;
        }

        List<Vector2Int> path = _pathCalculator.GetPath(currentUnit.CurrentTile.TileCoordinates, aimedTile.TileCoordinates, useDiagonals);
        if(path.Count > 0)
        {
            BattleTile[] pathTiles = new BattleTile[path.Count - 1];

            for (int i = 1; i < path.Count; i++)
            {
                pathTiles[i - 1] = battleRoom.PlacedBattleTiles[path[i].x, path[i].y];
            }

            StartCoroutine(currentUnit.MoveUnitCoroutine(pathTiles));
            _tilesManager.ResetTiles();

            StartCoroutine(CameraManager.Instance.FocusOnTrCoroutine(pathTiles[pathTiles.Length - 1].transform, 5f, 0.4f));

            yield return new WaitForSeconds(path.Count * 0.1f);
        }

        if(CurrentUnit.GetType() == typeof(Hero))
        {
            (CurrentUnit as Hero).DoAction();
            if ((currentUnit as Hero).CurrentActionPoints > 0)
            {
                _playerActionsMenu.OpenActionsMenu();
                yield break;
            }

            currentUnit.EndTurn(0.5f);
        }
    }

    #endregion


    #region Use Skill

    public IEnumerator UseSkillCoroutine(SkillData usedSkill)
    {
        if (usedSkill == null) usedSkill = _tilesManager.CurrentSkill;
        OnSkillUsed.Invoke();

        BattleTile[] skillBattleTiles = _tilesManager.GetAllSkillTiles().ToArray();
        currentVFXIndex = 0;

        // We verify is the attack is crit
        bool isCrit = false;
        if (currentUnit.GetType() == typeof(Hero) && usedSkill.skillEffects[0].skillEffectType == SkillEffectType.Damage)
        {
            Unit[] attackUnits = _tilesManager.GetConcernedUnits(skillBattleTiles, usedSkill, CurrentUnit);
            isCrit = currentUnit.VerifyCrit(attackUnits, _passivesManager.GetPassiveCritChanceUpgrade(currentUnit, attackUnits[0]));
        }

        // We launch the animations / others effects
        StartCoroutine(CameraManager.Instance.DoAttackFeelCoroutine(skillBattleTiles, CurrentUnit._unitAnimsInfos, isCrit));
        CurrentUnit._animator.SetBool("IsCrit", isCrit);
        CurrentUnit._animator.SetTrigger(usedSkill.animName);

        // We wait the moment the skill applies it's effect
        int wantedAttackCount = usedSkill.attackCount, currentAttackCount = 0;
        bool applied = false;
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (CurrentUnit._unitAnimsInfos.PlaySkillEffect && !applied)
            {
                currentAttackCount++;
                applied = true;

                // We apply the skill effect
                PlaySkillVFX(skillBattleTiles, usedSkill);
                for (int i = 0; i < skillBattleTiles.Length; i++)
                {
                    ApplySkillOnTile(skillBattleTiles[i], usedSkill, currentUnit, isCrit);
                }

                if (currentAttackCount >= wantedAttackCount) break;
            }


            if (!CurrentUnit._unitAnimsInfos.PlaySkillEffect)
            {
                applied = false;
            }
        }

        _tilesManager.ResetTiles();

        // We verify if the turn ends
        if (currentUnit.GetType() == typeof(Hero))
        {
            (currentUnit as Hero).CurrentSkillPoints -= usedSkill.skillPointCost;

            (currentUnit as Hero).DoAction();
            if ((currentUnit as Hero).CurrentActionPoints > 0)
            {
                yield return new WaitForSeconds(0.5f);

                _playerActionsMenu.OpenActionsMenu();
                yield break;
            }
        }

        currentUnit.EndTurn(0.5f);
    }


    private void ApplySkillOnTile(BattleTile battleTile, SkillData usedSkill, Unit unit, bool isCrit)
    {
        for (int i = 0; i < usedSkill.skillEffects.Length; i++)
        {
            if (usedSkill.skillEffects[i].onlyOnCrit && !isCrit) continue;

            // We verify the effect applies on the unit type on the tile
            switch (usedSkill.skillEffects[i].skillEffectTargetType)
            {
                case SkillEffectTargetType.Enemies:
                    if (battleTile.UnitOnTile is null) return;
                    if (battleTile.UnitOnTile.IsEnemy == currentUnit.IsEnemy) continue;
                    break;

                case SkillEffectTargetType.Allies:
                    if (battleTile.UnitOnTile is null) return;
                    if (battleTile.UnitOnTile.IsEnemy != currentUnit.IsEnemy) continue;
                    break;

                case SkillEffectTargetType.Self:
                    if (battleTile.UnitOnTile is null) return;
                    if (battleTile.UnitOnTile != unit) continue;
                    break;

                case SkillEffectTargetType.Empty:
                    if (battleTile.UnitOnTile is not null) continue;
                    break;
            }

            if (usedSkill.skillEffects[i].appliedAlteration != null)
            {
                battleTile.UnitOnTile.AddAlteration(usedSkill.skillEffects[i].appliedAlteration, unit);
            }

            // We apply the effect
            switch (usedSkill.skillEffects[i].skillEffectType)
            {
                case SkillEffectType.Damage:
                    if (isCrit) battleTile.UnitOnTile.TakeDamage((int)(usedSkill.skillEffects[i].multipliedPower * unit.CurrentStrength * 
                        (unit.CurrentCritMultiplier + _passivesManager.GetPassiveCritDamagesUpgrade(unit, battleTile.UnitOnTile))), unit);
                    else battleTile.UnitOnTile.TakeDamage((int)(usedSkill.skillEffects[i].multipliedPower * unit.CurrentStrength), unit);

                    PassiveData[] prokedPassives = _passivesManager.GetTriggeredPassives(PassiveTriggerType.OnAttack, unit).ToArray();
                    _passivesManager.ApplyPassives(prokedPassives, unit, battleTile.UnitOnTile);

                    prokedPassives = _passivesManager.GetTriggeredPassives(PassiveTriggerType.OnDamageReceived, battleTile.UnitOnTile).ToArray();
                    _passivesManager.ApplyPassives(prokedPassives, battleTile.UnitOnTile, unit);
                    break;

                case SkillEffectType.Heal:
                    battleTile.UnitOnTile.Heal(usedSkill.skillEffects[i].additivePower);
                    break;

                case SkillEffectType.Push:
                    battleTile.UnitOnTile.PushUnit(battleTile.TileCoordinates - currentUnit.CurrentTile.TileCoordinates, usedSkill.skillEffects[i].additivePower);
                    break;

                case SkillEffectType.Summon:
                    SummonUnit(usedSkill.skillEffects[i].summonPrefab, battleTile);
                    break;

                case SkillEffectType.HealDebuffs:
                    battleTile.UnitOnTile.RemoveAllNegativeAlterations();
                    break;

                case SkillEffectType.DestroySelf:
                    currentUnit.RemoveAllAlterations();
                    currentUnit.TakeDamage(999, currentUnit);
                    break;
            }
        }
    }

    private void SummonUnit(AIUnit prefab, BattleTile tile)
    {
        AIUnit unit = Instantiate(prefab, transform);
        unit.Initialise(false);
        unit.MoveUnit(tile);

        AddUnit(unit);
        unit.EnterBattle(tile);

        _timeline.AddUnit(unit);

        PassiveData[] passives = _passivesManager.GetTriggeredPassives(PassiveTriggerType.OnSummon, currentUnit).ToArray();
        _passivesManager.ApplyPassives(passives, currentUnit, unit);
    }

    #endregion


    #region Skill Visuals

    private void PlaySkillVFX(BattleTile[] battleTile, SkillData usedSkill)
    {
        if (usedSkill.VFXs.Length == 0 && usedSkill.downVFX == null && usedSkill.throwedObject == null) return;

        // Throwable VFX
        if (usedSkill.throwedObject != null)
        {
            Vector3 middlePos = Vector2.zero;
            foreach (BattleTile tile in battleTile)
            {
                middlePos += tile.transform.position;
            }
            middlePos /= battleTile.Length;

            if (usedSkill.throwedObject.TryGetComponent<Flask>(out Flask flask))
            {
                FlaskType type = FlaskType.Poison;
                if (usedSkill.skillEffects[0].appliedAlteration is null) type = FlaskType.Cure;
                else if (usedSkill.skillEffects[0].appliedAlteration.alterationType == AlterationType.Weakened) type = FlaskType.Debuff;
                Instantiate(flask, currentUnit.transform.position, Quaternion.Euler(0, 0, 0)).Initialise(middlePos, type);
            }
            return;
        }


        Vector2Int coordDif = _tilesManager.CurrentSkillBaseTile.TileCoordinates - currentUnit.CurrentTile.TileCoordinates;

        // We Choose the VFX to play
        GameObject usedVFX = null;
        if (usedSkill.VFXs.Length == 0)
        {
            if (coordDif.x > 0) usedVFX = usedSkill.rightVFX;
            else if (coordDif.x < 0) usedVFX = usedSkill.leftVFX;
            else if (coordDif.y > 0) usedVFX = usedSkill.upVFX;
            else usedVFX = usedSkill.downVFX;
        }
        else
        {
            usedVFX = usedSkill.VFXs[currentVFXIndex];
        }

        // We play the VFX
        if (usedSkill.VFXs.Length > currentVFXIndex && !usedSkill.onTargetVFX && usedSkill.oneVFXPerTile)
        {
            foreach (BattleTile tile in battleTile)
            {
                GameObject newVFX = Instantiate(usedVFX, tile.UnitOnTile.transform.position, Quaternion.Euler(0, 0, 0));
                AdaptVFXVisuals(newVFX, coordDif, usedSkill);
            }
        }
        else if (usedSkill.VFXs.Length > currentVFXIndex && usedSkill.onTargetVFX)
        {
            foreach (BattleTile tile in battleTile)
            {
                if (tile.UnitOnTile is null) continue;
                GameObject newVFX = Instantiate(usedVFX, tile.UnitOnTile.transform.position, Quaternion.Euler(0, 0, 0));
                AdaptVFXVisuals(newVFX, coordDif, usedSkill);
            }
        }
        else
        {
            Vector3 middlePos = Vector2.zero;
            foreach (BattleTile tile in battleTile)
            {
                middlePos += tile.transform.position;
            }
            middlePos /= battleTile.Length;

            GameObject newVFX = Instantiate(usedVFX, middlePos, Quaternion.Euler(0, 0, 0));
            AdaptVFXVisuals(newVFX, coordDif, usedSkill);
        }

        currentVFXIndex++;
    }

    private void AdaptVFXVisuals(GameObject vfx, Vector2Int coordDif, SkillData skillData)
    {
        if (skillData.mirrorHorizontalVFX)
        {
            if (coordDif.x < 0)
                vfx.transform.localScale = new Vector3(-1 * vfx.transform.localScale.x, vfx.transform.localScale.y, vfx.transform.localScale.z);
        }

        if (skillData.mirrorVerticalVFX)
        {
            if (coordDif.y > 0)
                vfx.transform.localScale = new Vector3(vfx.transform.localScale.x, -1 * vfx.transform.localScale.y, vfx.transform.localScale.z);
        }

        if (skillData.rotateVFX)
        {
            vfx.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(coordDif.y, coordDif.x) * Mathf.Rad2Deg);
        }
    }

    #endregion
}
