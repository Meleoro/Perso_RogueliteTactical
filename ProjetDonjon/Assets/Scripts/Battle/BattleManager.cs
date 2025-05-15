using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utilities;

public class BattleManager : GenericSingletonClass<BattleManager>
{
    [Header("Actions")]
    public Action OnMoveUnit;
    public Action OnSkillUsed;
    public Action OnBattleEnd;

    [Header("Private Infos")]
    private bool isInBattle;
    private List<Hero> currentHeroes = new();
    private List<Enemy> currentEnemies = new();
    private List<Unit> currentUnits = new();
    private Room battleRoom;
    private Unit currentUnit;
    private SkillData currentSkill;
    private BattleTile currentSkillBaseTile;

    [Header("Public Infos")]
    public Room BattleRoom { get {  return battleRoom; } }
    public Unit CurrentUnit { get { return currentUnit; } }
    public List<Hero> CurrentHeroes { get { return currentHeroes; } }
    public List<Enemy> CurrentEnemies { get { return currentEnemies; } }
    public bool IsInBattle {  get { return isInBattle; } }
    public MenuType CurrentActionType { get { return _playerActionsMenu.CurrentMenu; } }


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
            currentHeroes.Add((Hero)unit);
        }
        else
        {
            currentEnemies.Add((Enemy)unit);
        }
    }

    public void RemoveUnit(Unit unit)
    {
        _timeline.RemoveUnit(unit);
        currentUnits.Remove(unit);

        if (unit.GetType() == typeof(Hero))
        {
            currentHeroes.Remove((Hero)unit);
        }
        else 
        {
            currentEnemies.Remove((Enemy)unit);
        }

        if(currentEnemies.Count == 0)
        {
            //EndBattle();
        }
        else if(currentHeroes.Count == 0)
        {
            //GameOver
        }
    }

    #endregion


    #region Start / End Battle

    public void StartBattle(List<BattleTile> possibleTiles, Vector3 battleCenterPos, float cameraSize, Room battleRoom)
    {
        currentUnits.Clear();
        currentHeroes.Clear();
        currentEnemies.Clear();
        _pathCalculator.InitialisePathCalculator(battleRoom.PlacedBattleTiles);

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

        _timeline.InitialiseTimeline(currentUnits);

        CameraManager.Instance.EnterBattle(battleCenterPos, cameraSize);
        HeroesManager.Instance.EnterBattle(possibleTiles);

        UIManager.Instance.ShowHeroInfosPanels();

        StartCoroutine(StartBattleCoroutine());
    }

    private IEnumerator StartBattleCoroutine()
    {
        yield return new WaitForSeconds(1f);

        StartCoroutine(BattleManager.Instance.NextTurnCoroutine(0, false));
    }


    private void EndBattle()
    {
        isInBattle = false;

        battleRoom.EndBattle();

        UIManager.Instance.HideHeroInfosPanels();

        CameraManager.Instance.ExitBattle();
        HeroesManager.Instance.ExitBattle();

        _timeline.Disappear();
    }

    #endregion


    #region Battle Events

    public IEnumerator NextTurnCoroutine(float delay = 0, bool endTurn = true)
    {
        yield return new WaitForSeconds(delay);

        if(endTurn)
            _timeline.NextTurn();

        currentUnit = _timeline.Slots[0].Unit;
        currentUnit.StartTurn();

        CameraManager.Instance.FocusOnTr(currentUnit.transform, 5f);

        if (currentUnit.GetType() == typeof(Hero))
        {
            _playerActionsMenu.SetupHeroActionsUI(currentUnit as Hero);
        }
        else
        {
            Enemy enemy = currentUnit as Enemy;
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
        BattleTile[] pathTiles =  new BattleTile[path.Count];

        for(int i = 0; i < path.Count; i++)
        {
            pathTiles[i] = battleRoom.PlacedBattleTiles[path[i].x, path[i].y];
        }

        StartCoroutine(currentUnit.MoveUnitCoroutine(pathTiles));
        ResetTiles();

        StartCoroutine(CameraManager.Instance.FocusOnTrCoroutine(pathTiles[pathTiles.Length-1].transform, 5f, 0.4f));

        yield return new WaitForSeconds(path.Count * 0.1f);

        if(CurrentUnit.GetType() == typeof(Hero)) 
            OnMoveUnit.Invoke();
    }


    public IEnumerator UseSkillCoroutine(SkillData usedSkill)
    {
        if (usedSkill == null)
            usedSkill = currentSkill;

        OnSkillUsed.Invoke();

        CurrentUnit._animator.SetTrigger(usedSkill.animName);

        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (CurrentUnit._unitAnimsInfos.PlaySkillEffect)
                break;
        }

        BattleTile[] skillBattleTiles = GetAllSkillTiles().ToArray();
        for (int i = 0; i < skillBattleTiles.Length; i++)
        {
            ApplySkillOnTile(skillBattleTiles[i], usedSkill, currentUnit);
        }

        ResetTiles();
        
        StartCoroutine(NextTurnCoroutine(1f));
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


    private void ApplySkillOnTile(BattleTile battleTile, SkillData usedSkill, Unit unit)
    {
        if (battleTile.UnitOnTile is null) return;

        for (int i = 0; i < usedSkill.skillEffects.Length; i++)
        {
            if (battleTile.UnitOnTile is null) return;

            // We verify the effect applies on the unit type on the tile
            switch (usedSkill.skillEffects[i].skillEffectTargetType)
            {
                case SkillEffectTargetType.Enemies:
                    if (battleTile.UnitOnTile.GetType() == currentUnit.GetType()) continue;
                    break;

                case SkillEffectTargetType.Allies:
                    if (battleTile.UnitOnTile.GetType() != currentUnit.GetType()) continue;
                    break;

                case SkillEffectTargetType.Self:
                    if (battleTile.UnitOnTile != unit) continue;
                    break;
            }



            // We apply the effect
            switch (usedSkill.skillEffects[i].skillEffectType)
            {
                case SkillEffectType.Damage:
                    battleTile.UnitOnTile.TakeDamage((int)(usedSkill.skillEffects[i].multipliedPower * unit.CurrentStrength));
                    break;

                case SkillEffectType.Heal:

                    break;

                case SkillEffectType.AddShield:
                    battleTile.UnitOnTile.AddShield(usedSkill.skillEffects[i].additivePower);
                    break;

                case SkillEffectType.ModifyMove:
                    battleTile.UnitOnTile.AddStatsModificator(usedSkill.skillEffects[i].skillEffectType, usedSkill.skillEffects[i].additivePower, usedSkill.skillEffects[i].multipliedPower, usedSkill.skillEffects[i].duration, currentUnit);
                    break;

                case SkillEffectType.ModifySpeed:
                    battleTile.UnitOnTile.AddStatsModificator(usedSkill.skillEffects[i].skillEffectType, usedSkill.skillEffects[i].additivePower, usedSkill.skillEffects[i].multipliedPower, usedSkill.skillEffects[i].duration, currentUnit);
                    break;

                case SkillEffectType.ModifyStrength:
                    battleTile.UnitOnTile.AddStatsModificator(usedSkill.skillEffects[i].skillEffectType, usedSkill.skillEffects[i].additivePower, usedSkill.skillEffects[i].multipliedPower, usedSkill.skillEffects[i].duration, currentUnit);
                    break;

                case SkillEffectType.ModifyLuck:
                    battleTile.UnitOnTile.AddStatsModificator(usedSkill.skillEffects[i].skillEffectType, usedSkill.skillEffects[i].additivePower, usedSkill.skillEffects[i].multipliedPower, usedSkill.skillEffects[i].duration, currentUnit);
                    break;

                case SkillEffectType.Push:
                    battleTile.UnitOnTile.PushUnit(battleTile.TileCoordinates - currentUnit.CurrentTile.TileCoordinates, usedSkill.skillEffects[i].additivePower);
                    break;

                case SkillEffectType.Provoke:
                    battleTile.UnitOnTile.AddStatsModificator(usedSkill.skillEffects[i].skillEffectType, usedSkill.skillEffects[i].additivePower, usedSkill.skillEffects[i].multipliedPower, usedSkill.skillEffects[i].duration, currentUnit);
                    break;
            }
        }
    }

    #endregion


    #region Tiles Functions

    public List<BattleTile> GetPaternTiles(Vector2Int paternMiddle, bool[] patern, int paternSize)
    {
        List<BattleTile> returnedTiles = new List<BattleTile>();

        for (int i = 0; i < patern.Length; i++) 
        {
            if (!patern[i]) continue;

            Vector2Int currentCoord = new Vector2Int(i / paternSize, i % paternSize);
            Vector2Int coordAccordingToCenter = new Vector2Int(currentCoord.x - (int)(paternSize * 0.5f), currentCoord.y - (int)(paternSize * 0.5f));
            BattleTile battleTile = battleRoom.GetBattleTile(paternMiddle + coordAccordingToCenter);

            if (battleTile is not null) returnedTiles.Add(battleTile);
        }

        return returnedTiles;
    }


    public void DisplayPossibleSkillTiles(SkillData skill, BattleTile baseTile, bool doBounce = true)
    {
        ResetTiles();

        if (skill is not null) currentSkill = skill;
        if (baseTile is not null) currentSkillBaseTile = baseTile;

        List<BattleTile> skillTiles = GetPaternTiles(currentSkillBaseTile.TileCoordinates, currentSkill.skillPatern, (int)Mathf.Sqrt(currentSkill.skillPatern.Length));

        for(int i = 0; i < skillTiles.Count; i++)
        {
            skillTiles[i].DisplayPossibleAttackTile(doBounce);
        }
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
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternVertical, (int)Mathf.Sqrt(skill.skillAOEPaternVertical.Length));

                    else
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternHorizontal, (int)Mathf.Sqrt(skill.skillAOEPaternHorizontal.Length));
                }
                else
                {
                    skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPatern, (int)Mathf.Sqrt(skill.skillAOEPatern.Length));
                }
                break;

            case SkillType.SkillArea:
                skillTiles = GetPaternTiles(currentUnit.CurrentTile.TileCoordinates, skill.skillPatern, (int)Mathf.Sqrt(skill.skillPatern.Length));
                break;
        }

        for (int i = 0; i < skillTiles.Count; i++)
        {
            skillTiles[i].DisplayDangerTile();
        }
    }


    public void DisplayPossibleMoveTiles(Enemy enemy)
    {
        ResetTiles();

        List<BattleTile> moveTills = GetPaternTiles(enemy.CurrentTile.TileCoordinates, enemy.EnemyData.movePatern, (int)Mathf.Sqrt(enemy.EnemyData.movePatern.Length));
        for (int i = 0; i < moveTills.Count; i++)
        {
            moveTills[i].DisplayMoveTile();
        }
    }

    public void StopDisplayPossibleMoveTiles()
    {
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

                openList.Add(neighbors[j]);
                closedList.Add(neighbors[j]);

                neighbors[j].DisplayMoveTile();
            }
        }

        CameraManager.Instance.FocusOnPosition(tilesPositionsAddition / tilesCount, 6f);
    }


    public void ResetTiles()
    {
        for(int i = 0; i < battleRoom.BattleTiles.Count; i++)
        {
            battleRoom.BattleTiles[i].DisplayNormalTile();
        }
    }

    #endregion


    #region Others



    #endregion
}
