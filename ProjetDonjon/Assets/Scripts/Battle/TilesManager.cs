using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class TilesManager 
{
    [Header("Private Infos")]
    public SkillData currentSkill;
    public BattleTile currentSkillBaseTile;
    public Unit currentUnit;
    public Room battleRoom;

    [Header("References")]
    private PathCalculator _pathCalculator;
    private PlayerActionsMenu _playerActionsMenu;



    public void Initialise(PathCalculator pathCalculator, PlayerActionsMenu actionMenu)
    {
        _pathCalculator = pathCalculator;
        _playerActionsMenu = actionMenu;
    }


    #region Tiles Functions

    public List<BattleTile> GetPaternTiles(Vector2Int paternMiddle, bool[] patern, int paternSize, bool useDiagonals = false,
        ObstacleType obstacleType = ObstacleType.Nothing, BattleTile ignoredTile = null)
    {
        List<BattleTile> returnedTiles = new List<BattleTile>();

        for (int i = 0; i < patern.Length; i++)
        {
            if (!patern[i]) continue;

            Vector2Int currentCoord = new Vector2Int(i % paternSize, i / paternSize);
            Vector2Int coordAccordingToCenter = new Vector2Int(currentCoord.x - (int)(paternSize * 0.5f), currentCoord.y - (int)(paternSize * 0.5f));
            BattleTile battleTile = battleRoom.GetBattleTile(paternMiddle + coordAccordingToCenter);

            if (battleTile is null) continue;
            if (obstacleType != ObstacleType.Nothing && !_pathCalculator.VerifyIsReachable(paternMiddle, paternMiddle + coordAccordingToCenter,
                useDiagonals, obstacleType, ignoredTile)) continue;
            if (battleTile.UnitOnTile is not null && (obstacleType == ObstacleType.Units || obstacleType == ObstacleType.All)
                && ignoredTile != battleTile) continue;

            returnedTiles.Add(battleTile);
        }

        return returnedTiles;
    }

    public List<BattleTile> GetAdjacentTiles(BattleTile middleTile)
    {
        List<BattleTile> result = new List<BattleTile>();
        List<BattleTile> openList = new List<BattleTile>();
        result.Add(middleTile);
        openList.Add(middleTile);

        while (openList.Count > 0)
        {
            BattleTile currentTile = openList[0];
            openList.RemoveAt(0);

            foreach (BattleTile tile in currentTile.TileNeighbors)
            {
                if (result.Contains(tile)) continue;
                if (tile.CurrentTileState != BattleTileState.Attack && tile.CurrentTileState != BattleTileState.Danger) continue;

                result.Add(tile);
                openList.Add(tile);
            }
        }

        return result;
    }


    public List<BattleTile> DisplayPossibleSkillTiles(SkillData skill, BattleTile baseTile, bool doBounce = true)
    {
        ResetTiles();

        if (skill is not null) currentSkill = skill;
        if (baseTile is not null) currentSkillBaseTile = baseTile;

        bool isBlockedByUnits = currentSkill.skillType != SkillType.SkillArea && currentSkill.skillType != SkillType.AdjacentTiles;

        List<BattleTile> skillTiles = GetPaternTiles(currentSkillBaseTile.TileCoordinates, currentSkill.skillPatern,
            (int)Mathf.Sqrt(currentSkill.skillPatern.Length), true,
            isBlockedByUnits ? ObstacleType.UnitsIncluded : ObstacleType.Nothing);

        for (int i = 0; i < skillTiles.Count; i++)
        {
            skillTiles[i].DisplayPossibleAttackTile(doBounce);
        }

        return skillTiles;
    }


    public void DisplayDangerTiles(BattleTile overlayedTile, SkillData skill)
    {
        // For Heroes
        if (skill is null)
        {
            skill = currentSkill;
            //DisplayPossibleSkillTiles(skill, currentSkillBaseTile);
        }

        List<BattleTile> skillTiles = new List<BattleTile>();

        switch (skill.skillType)
        {
            case SkillType.AOEPaternTiles:
                if (skill.useOrientatedAOE)
                {
                    Vector2Int coordinateDif = overlayedTile.TileCoordinates - currentUnit.CurrentTile.TileCoordinates;

                    if (coordinateDif.y > 0)
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternUp,
                            (int)Mathf.Sqrt(skill.skillAOEPaternUp.Length), false, Enums.ObstacleType.UnitsIncluded);

                    else if (coordinateDif.y < 0)
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternDown,
                            (int)Mathf.Sqrt(skill.skillAOEPaternDown.Length), false, Enums.ObstacleType.UnitsIncluded);

                    else if (coordinateDif.x > 0)
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternRight,
                            (int)Mathf.Sqrt(skill.skillAOEPaternRight.Length), false, Enums.ObstacleType.UnitsIncluded);

                    else
                        skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPaternLeft,
                            (int)Mathf.Sqrt(skill.skillAOEPaternLeft.Length), false, Enums.ObstacleType.UnitsIncluded);
                }
                else
                {
                    skillTiles = GetPaternTiles(overlayedTile.TileCoordinates, skill.skillAOEPatern, (int)Mathf.Sqrt(skill.skillAOEPatern.Length), false,
                        ObstacleType.UnitsIncluded);
                }
                break;

            case SkillType.SkillArea:
                skillTiles = GetPaternTiles(currentUnit.CurrentTile.TileCoordinates, skill.skillPatern, (int)Mathf.Sqrt(skill.skillPatern.Length),
                    false, ObstacleType.Nothing);
                break;

            case SkillType.AdjacentTiles:
                skillTiles = GetAdjacentTiles(overlayedTile);
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
                    (int)Mathf.Sqrt(enemy.AIData.movePatern.Length), true, ObstacleType.All);

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
                    (int)Mathf.Sqrt(enemy.AIData.movePatern.Length), true, ObstacleType.All);

                for (int i = 0; i < tiles.Count; i++)
                {
                    secondaryTiles = GetPaternTiles(tiles[i].TileCoordinates, enemy.CurrentSkillData.skillPatern,
                        (int)Mathf.Sqrt(enemy.CurrentSkillData.skillPatern.Length), true, ObstacleType.UnitsIncluded);

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

        if (_playerActionsMenu.CurrentMenu == MenuType.Move)
        {
            DisplayPossibleMoveTiles(currentUnit as Hero);
        }
    }


    public void DisplayPossibleMoveTiles(Hero hero)
    {
        BattleTile startTile = hero.CurrentTile;
        List<BattleTile> openList = new List<BattleTile>();
        List<BattleTile> closedList = new List<BattleTile>();
        Vector3 tilesPositionsAddition = Vector3.zero;
        int tilesCount = 0;

        openList.Add(startTile);
        closedList.Add(startTile);

        for (int i = 0; i < hero.CurrentMovePoints; i++)
        {
            List<BattleTile> neighbors = new List<BattleTile>();

            for (int j = 0; j < openList.Count; j++)
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
}
