using System.Collections;
using System.Linq;
using UnityEngine;

public class Enemy : Unit
{
    [Header("Parameters")]
    [SerializeField] private EnemyData enemyData;

    [Header("Private Infos")]
    private BattleTile[] aimedTiles;
    private SkillData currentSkillData;

    [Header("Public Infos")]
    public EnemyData EnemyData { get { return enemyData; } }



    private void Start()
    {
        unitData = enemyData;
        currentSkillData = enemyData.skills[0];

        InitialiseUnitInfos(enemyData.baseHealth, enemyData.baseStrength, enemyData.baseSpeed, enemyData.baseLuck, 0);
    }


    private void SetupAimedTiles()
    {
        if(provocationTarget is not null)
        {
            aimedTiles = new BattleTile[1];
            aimedTiles[0] = provocationTarget.CurrentTile;

            return;
        }

        aimedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];

        for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++)
        {
            aimedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
        }
    }

    public IEnumerator PlayEnemyTurnCoroutine()
    {
        SetupAimedTiles();

        yield return new WaitForSeconds(0.5f);

        BattleTile moveTile = null;
        BattleTile skillTile = null;
        (moveTile, skillTile) = GetBestMove(currentTile);

        if(skillTile is null)
        {
            (moveTile, skillTile) = GetBestMove(currentTile, 0, 1);
            skillTile = null;
        }

        StartCoroutine(BattleManager.Instance.MoveUnitCoroutine(moveTile, true));

        yield return new WaitForSeconds(0.5f);

        if(skillTile is not null)
        {
            BattleManager.Instance.DisplayDangerTiles(skillTile, currentSkillData);

            yield return new WaitForSeconds(0.5f);

            StartCoroutine(BattleManager.Instance.UseSkillCoroutine(currentSkillData));

            yield return new WaitForSeconds(0.5f);

            //BattleManager.Instance.OnSkillApplied += EndTurn;
        }
        else
        {
            EndTurn();
        }
    }

    
    private (BattleTile, BattleTile) GetBestMove(BattleTile currentTile, int depth = 0, int maxDepth = 0)
    {
        BattleTile[] possibleMoves = BattleManager.Instance.GetPaternTiles(currentTile.TileCoordinates, enemyData.movePatern, (int)Mathf.Sqrt(enemyData.movePatern.Length)).ToArray();
        int bestGrade = -1000;
        BattleTile pickedMoveTile = currentTile;
        BattleTile pickedSkillTile = null;

        for (int i = 0; i < possibleMoves.Length; i++)
        {
            if (possibleMoves[i].UnitOnTile is not null)
            {
                if (possibleMoves[i].UnitOnTile != this) continue;
            }

            if(depth < maxDepth)
            {
                BattleTile currentMoveTile = null;
                BattleTile currentSkillTile = null;

                (currentMoveTile, currentSkillTile) = GetBestMove(possibleMoves[i], depth+1, maxDepth);

                if (currentSkillTile is null) continue;
                if (currentSkillTile.UnitOnTile is null) continue;
                if (currentSkillTile.UnitOnTile.GetType() != typeof(Hero)) continue;

                int moveGrade = GetMoveGrade(currentMoveTile.TileCoordinates, currentSkillTile.TileCoordinates, depth);

                if (moveGrade < bestGrade) continue;

                bestGrade = moveGrade;
                pickedMoveTile = possibleMoves[i];
            }
            else
            {
                BattleTile[] possibleSkillTiles = BattleManager.Instance.GetPaternTiles(possibleMoves[i].TileCoordinates, currentSkillData.skillPatern, (int)Mathf.Sqrt(currentSkillData.skillPatern.Length)).ToArray();

                for (int j = 0; j < possibleSkillTiles.Length; j++)
                {
                    int moveGrade = GetMoveGrade(possibleMoves[i].TileCoordinates, possibleSkillTiles[j].TileCoordinates, depth);
                    if (moveGrade > bestGrade)
                    {
                        bestGrade = moveGrade;
                        pickedMoveTile = possibleMoves[i];

                        if (possibleSkillTiles[j].UnitOnTile is null) continue;
                        if (possibleSkillTiles[j].UnitOnTile.GetType() != typeof(Hero)) continue;

                        pickedSkillTile = possibleSkillTiles[j];
                    }
                }
            }
        }

        return (pickedMoveTile, pickedSkillTile);
    }

    private int GetMoveGrade(Vector2Int testedMovePos, Vector2Int testedSkillPos, int depth)
    {
        int finalGrade = -5 * depth;

        // Move grade
        for (int i = 0; i < aimedTiles.Length; i++)
        {
            int currentDist = (int)Vector2Int.Distance(testedMovePos, aimedTiles[i].TileCoordinates);

            switch (enemyData.AI)
            {
                case AIType.Classic:
                    finalGrade -= currentDist;
                    break;

                case AIType.Shy:
                    finalGrade += currentDist;
                    break;
            }
        }

        // Skill Grade
        BattleTile[] skillDangerTiles = BattleManager.Instance.GetPaternTiles(testedSkillPos, currentSkillData.skillAOEPatern, (int)Mathf.Sqrt(currentSkillData.skillAOEPatern.Length)).ToArray();

        for (int i = 0; i < skillDangerTiles.Length; i++)
        {
            if (!aimedTiles.Contains(skillDangerTiles[i])) continue;

            finalGrade += 20;
        }

        return finalGrade;
    }
}
