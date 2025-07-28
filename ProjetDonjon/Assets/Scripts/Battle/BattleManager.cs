using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

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

    [Header("Public Infos")]
    public Room BattleRoom { get {  return battleRoom; } }
    public Unit CurrentUnit { get { return currentUnit; } }
    public List<Unit> CurrentHeroes { get { return currentHeroes; } }
    public List<AIUnit> CurrentEnemies { get { return currentEnemies; } }
    public bool IsInBattle {  get { return isInBattle; } }
    public MenuType CurrentActionType { get { return _playerActionsMenu.CurrentMenu; } }
    public Tile[] HoleTiles { get { return holeTiles; } }

    [Header("References")]
    [SerializeField] private Timeline _timeline;
    [SerializeField] private PlayerActionsMenu _playerActionsMenu;
    public PathCalculator _pathCalculator;


    private void Start()
    {
        _pathCalculator = new PathCalculator();
    }


    private void Update()
    {
        if (!isInBattle) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            EndBattle();
        }
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

        for(int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
        {
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
        yield return new WaitForSeconds(1f + delay);

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
            deadHeroes[i].HideOutline();
            deadHeroes[i].Heal(1);
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

    #endregion


    #region Battle Events

    public IEnumerator NextTurnCoroutine(float delay = 0, bool endTurn = true)
    {
        yield return new WaitForSeconds(delay);

        if (!isInBattle) yield break;

        if(endTurn)
            _timeline.NextTurn();

        currentUnit = _timeline.Slots[0].Unit;
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
            ResetTiles();

            if (CurrentUnit.GetType() == typeof(Hero))
                OnMoveUnit.Invoke();

            yield break;
        }

        List<Vector2Int> path = _pathCalculator.GetPath(currentUnit.CurrentTile.TileCoordinates, aimedTile.TileCoordinates, useDiagonals);
        BattleTile[] pathTiles =  new BattleTile[path.Count - 1];

        for(int i = 1; i < path.Count; i++)
        {
            pathTiles[i-1] = battleRoom.PlacedBattleTiles[path[i].x, path[i].y];
        }

        StartCoroutine(currentUnit.MoveUnitCoroutine(pathTiles));
        ResetTiles();

        StartCoroutine(CameraManager.Instance.FocusOnTrCoroutine(pathTiles[pathTiles.Length-1].transform, 5f, 0.4f));

        yield return new WaitForSeconds(path.Count * 0.1f);

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


    public Unit[] GetConcernedUnits(BattleTile[] tiles, SkillData skill, Unit baseUnit)
    {
        List<Unit> result = new List<Unit>();

        foreach(BattleTile tile in tiles)
        {
            if (tile.UnitOnTile is null) continue;

            switch (skill.skillEffects[0].skillEffectTargetType)
            {
                case SkillEffectTargetType.Enemies:
                    if (baseUnit.GetType() != tile.UnitOnTile.GetType()) result.Add(tile.UnitOnTile);
                    break;

                case SkillEffectTargetType.Allies:
                    if (baseUnit.GetType() == tile.UnitOnTile.GetType()) result.Add(tile.UnitOnTile);
                    break;
            }
        }

        return result.ToArray();
    }

    public IEnumerator UseSkillCoroutine(SkillData usedSkill)
    {
        if (usedSkill == null) usedSkill = currentSkill;
        OnSkillUsed.Invoke();

        BattleTile[] skillBattleTiles = GetAllSkillTiles().ToArray();

        // We verify is the attack is crit
        bool isCrit = false;
        if (currentUnit.GetType() == typeof(Hero) && currentSkill.skillEffects[0].skillEffectType == SkillEffectType.Damage)
        {
            Unit[] attackUnits = GetConcernedUnits(skillBattleTiles, usedSkill, CurrentUnit);
            isCrit = currentUnit.VerifyCrit(attackUnits);
        }

        // We launch the animations / others effects
        StartCoroutine(CameraManager.Instance.DoAttackFeelCoroutine(CurrentUnit._unitAnimsInfos, isCrit));
        CurrentUnit._animator.SetBool("IsCrit", isCrit);
        CurrentUnit._animator.SetTrigger(usedSkill.animName);

        // We wait the moment the skill applies it's effect
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (CurrentUnit._unitAnimsInfos.PlaySkillEffect)
                break;
        }

        // We apply the skill effect
        for (int i = 0; i < skillBattleTiles.Length; i++)
        {
            ApplySkillOnTile(skillBattleTiles[i], usedSkill, currentUnit, isCrit);
        }

        ResetTiles();

        // We verify if the turn ends
        if(currentUnit.GetType() == typeof(Hero))
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


    private List<BattleTile> GetAllSkillTiles()
    {
        List<BattleTile> returnedTiles = new List<BattleTile>();

        for(int i = 0; i < battleRoom.BattleTiles.Count; i++)
        {
            if (battleRoom.BattleTiles[i].CurrentTileState == BattleTileState.Danger)
            {
                returnedTiles.Add(battleRoom.BattleTiles[i]);
            }
        } 

        return returnedTiles;
    }


    private void ApplySkillOnTile(BattleTile battleTile, SkillData usedSkill, Unit unit, bool isCrit)
    {
        if(usedSkill.VFX != null)
        {
            Instantiate(usedSkill.VFX, battleTile.UnitOnTile.transform.position, Quaternion.Euler(0, 0, 0));
        }

        for (int i = 0; i < usedSkill.skillEffects.Length; i++)
        {
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
                    // Thorn
                    if (isCrit) battleTile.UnitOnTile.TakeDamage((int)(usedSkill.skillEffects[i].multipliedPower 
                        * unit.CurrentStrength * unit.CurrentCritMultiplier), unit);
                    else battleTile.UnitOnTile.TakeDamage((int)(usedSkill.skillEffects[i].multipliedPower * unit.CurrentStrength), unit);
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
            }
        }
    }

    private void SummonUnit(AIUnit prefab, BattleTile tile)
    {
        AIUnit unit = Instantiate(prefab, transform);
        unit.MoveUnit(tile);

        AddUnit(unit);
        unit.EnterBattle(tile);

        _timeline.AddUnit(unit);
    }

    #endregion


    #region Tiles Functions

    public List<BattleTile> GetPaternTiles(Vector2Int paternMiddle, bool[] patern, int paternSize, bool checkReachable = true, bool useDiagonals = false, bool blockedByUnits = false, BattleTile ignoredTile = null)
    {
        List<BattleTile> returnedTiles = new List<BattleTile>();

        for (int i = 0; i < patern.Length; i++) 
        {
            if (!patern[i]) continue;

            Vector2Int currentCoord = new Vector2Int(i / paternSize, i % paternSize);
            Vector2Int coordAccordingToCenter = new Vector2Int(currentCoord.x - (int)(paternSize * 0.5f), currentCoord.y - (int)(paternSize * 0.5f));
            BattleTile battleTile = battleRoom.GetBattleTile(paternMiddle + coordAccordingToCenter);

            if (checkReachable && !_pathCalculator.VerifyIsReachable(paternMiddle, paternMiddle + coordAccordingToCenter, useDiagonals, ignoredTile)) continue;
            if (battleTile is null) continue;
            if (battleTile.UnitOnTile is not null && blockedByUnits && ignoredTile != battleTile) continue;

            returnedTiles.Add(battleTile);
        }

        return returnedTiles;
    }


    public List<BattleTile> DisplayPossibleSkillTiles(SkillData skill, BattleTile baseTile, bool doBounce = true)
    {
        ResetTiles();

        if (skill is not null) currentSkill = skill;
        if (baseTile is not null) currentSkillBaseTile = baseTile;

        List<BattleTile> skillTiles = GetPaternTiles(currentSkillBaseTile.TileCoordinates, currentSkill.skillPatern, (int)Mathf.Sqrt(currentSkill.skillPatern.Length));

        for(int i = 0; i < skillTiles.Count; i++)
        {
            skillTiles[i].DisplayPossibleAttackTile(doBounce);
        }

        return skillTiles;
    }


    public void DisplayDangerTiles(BattleTile overlayedTile, SkillData skill)
    {
        // For Heroes
        if(skill is null)
        {
            skill = currentSkill;
            //DisplayPossibleSkillTiles(skill, currentSkillBaseTile);
        }

        List<BattleTile> skillTiles =new List<BattleTile>();

        switch(skill.skillType)
        {
            case SkillType.AOEPaternTiles:
                if (skill.useOrientatedAOE)
                {
                    Vector2Int coordinateDif = overlayedTile.TileCoordinates - CurrentUnit.CurrentTile.TileCoordinates;

                    if(coordinateDif.y != 0)
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternVertical, (int)Mathf.Sqrt(skill.skillAOEPaternVertical.Length), false);

                    else
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternHorizontal, (int)Mathf.Sqrt(skill.skillAOEPaternHorizontal.Length), false);
                }
                else
                {
                    skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPatern, (int)Mathf.Sqrt(skill.skillAOEPatern.Length), false);
                }
                break;

            case SkillType.SkillArea:
                skillTiles = GetPaternTiles(currentUnit.CurrentTile.TileCoordinates, skill.skillPatern, (int)Mathf.Sqrt(skill.skillPatern.Length), false);
                break;
        }

        for (int i = 0; i < skillTiles.Count; i++)
        {
            skillTiles[i].DisplayDangerTile();
        }
    }


    // FOR ENEMIES, SHOWS THE POSSIBLE MOVE / SKILL TILES 
    public void DisplayPossibleTiles(AIUnit enemy)
    {
        ResetTiles();

        List<BattleTile> tiles = new List<BattleTile>();
        List<BattleTile> secondaryTiles = new List<BattleTile>();
        switch (enemy.CurrentPreviewType)
        {
            case PreviewType.Move:
                tiles = GetPaternTiles(enemy.CurrentTile.TileCoordinates, enemy.AIData.movePatern, 
                    (int)Mathf.Sqrt(enemy.AIData.movePatern.Length), true, true);

                for (int i = 0; i < tiles.Count; i++)
                {
                    tiles[i].DisplayMoveTile();
                }

                break;

            case PreviewType.Attack:
                tiles = GetPaternTiles(enemy.CurrentTile.TileCoordinates, enemy.CurrentSkillData.skillPatern,
                    (int)Mathf.Sqrt(enemy.CurrentSkillData.skillPatern.Length), true);

                for (int i = 0; i < tiles.Count; i++)
                {
                    tiles[i].DisplayPossibleAttackTile(true);
                }
                break;

            case PreviewType.MoveAndAttack:
                tiles = GetPaternTiles(enemy.CurrentTile.TileCoordinates, enemy.AIData.movePatern,
                    (int)Mathf.Sqrt(enemy.AIData.movePatern.Length), true, true);

                for (int i = 0; i < tiles.Count; i++)
                {
                    secondaryTiles = GetPaternTiles(tiles[i].TileCoordinates, enemy.CurrentSkillData.skillPatern,
                        (int)Mathf.Sqrt(enemy.CurrentSkillData.skillPatern.Length), true);

                    for (int j = 0; j < secondaryTiles.Count; j++)
                    {
                        if (secondaryTiles[j].CurrentTileState == BattleTileState.Move) continue;
                        secondaryTiles[j].DisplayPossibleAttackTile(true);
                    }

                    tiles[i].DisplayMoveTile();
                }

                break;
        }
    }

    public void StopDisplayPossibleMoveTiles()
    {
        if (_playerActionsMenu.CurrentMenu == MenuType.Skills) return;

        ResetTiles();

        if(_playerActionsMenu.CurrentMenu == MenuType.Move)
        {
            DisplayPossibleMoveTiles(currentUnit as Hero);
        }
    }


    public void DisplayPossibleMoveTiles(Hero hero)
    {
        BattleTile startTile = hero.CurrentTile;
        List<BattleTile> openList = new List<BattleTile>();
        List<BattleTile> closedList= new List<BattleTile>();
        Vector3 tilesPositionsAddition = Vector3.zero;
        int tilesCount = 0;

        openList.Add(startTile);
        closedList.Add(startTile);

        for(int i = 0; i < hero.CurrentMovePoints; i++)
        {
            List<BattleTile> neighbors = new List<BattleTile>();

            for(int j = 0; j < openList.Count; j++)
            {
                neighbors.AddRange(openList[j].TileNeighbors);

                openList[j].DisplayMoveTile();

                tilesPositionsAddition += openList[j].transform.position;
                tilesCount++;
            }

            openList.Clear();

            for (int j = 0; j < neighbors.Count; j++)
            {
                if (closedList.Contains(neighbors[j])) continue;
                if (neighbors[j].UnitOnTile != null) continue;
                if (neighbors[j].IsHole) continue;

                openList.Add(neighbors[j]);
                closedList.Add(neighbors[j]);

                neighbors[j].DisplayMoveTile();
            }
        }

        CameraManager.Instance.FocusOnPosition(tilesPositionsAddition / tilesCount, 6f);
    }


    public void ResetTiles()
    {
        for (int i = 0; i < battleRoom.BattleTiles.Count; i++)
        {
            battleRoom.BattleTiles[i].DisplayNormalTile();
        }
    }

    #endregion


    #region Others

    public void StartBattleEndCutscene() 
    {
        isInBattleCutscene = true;
        isInBattle = false;

        UIManager.Instance.HideHeroInfosPanels();
        _playerActionsMenu.CloseActionsMenu();
    }

    #endregion
}
