using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

public enum PreviewType
{
    Move,
    Attack,
    MoveAndAttack
}

public class AIUnit : Unit
{
    [Header("Parameters")]
    [SerializeField] private AIData aiData;

    [Header("Private Infos")]
    private BattleTile[] aimedTiles;
    private BattleTile[] avoidedTiles;
    private SkillData currentSkillData;
    private PreviewType currentPreviewType;

    [Header("Public Infos")]
    public AIData AIData { get { return aiData; } }
    public SkillData CurrentSkillData { get { return currentSkillData; } }
    public PreviewType CurrentPreviewType { get { return currentPreviewType; } }

    #region Setup

    private void Start()
    {
        unitData = AIData;
        currentSkillData = AIData.skills[0];

        currentPreviewType = PreviewType.Move;

        InitialiseUnitInfos(AIData.baseHealth, AIData.baseStrength, AIData.baseSpeed, AIData.baseLuck, 0);
        StartCoroutine(AppearCoroutine(1f));
    }


    private void SetupAimedTiles()
    {
        if(provocationTarget is not null)
        {
            aimedTiles = new BattleTile[1];
            aimedTiles[0] = provocationTarget.CurrentTile;

            return;
        }

        if (isEnemy)
        {
            if (currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Allies)
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                    aimedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
                }
            }

            else
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++) {
                    aimedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
                }
            }

            avoidedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];
            for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++) {
                avoidedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
            }
        }

        else
        {
            if (currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Allies)
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++) {
                    aimedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
                }
            }

            else
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                    aimedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
                }
            }

            avoidedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
            for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                avoidedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
            }
        }
    }

    #endregion


    #region Appear / Disappear

    private IEnumerator AppearCoroutine(float duration)
    {
        _spriteRenderer.material.ULerpMaterialFloat(duration, 3.5f, "_DitherProgress");

        yield return new WaitForSeconds(duration);  
    }

    private IEnumerator DisappearCoroutine(float duration)
    {
        _spriteRenderer.material.ULerpMaterialFloat(duration, -0.5f, "_DitherProgress");

        yield return new WaitForSeconds(duration);

        base.Die();
    }

    #endregion


    #region AI Functions

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

            if (skillTile is null) {
                (moveTile, skillTile) = GetBestMove(currentTile, 0, 2);
            }

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
            EndTurn(0f);
        }
    }

    
    private (BattleTile, BattleTile) GetBestMove(BattleTile currentTile, int depth = 0, int maxDepth = 0)
    {
        BattleTile[] possibleMoves = 
            BattleManager.Instance.GetPaternTiles(currentTile.TileCoordinates, AIData.movePatern, (int)Mathf.Sqrt(AIData.movePatern.Length), true).ToArray();
        int bestGrade = -1000;
        BattleTile pickedMoveTile = currentTile;
        BattleTile pickedSkillTile = null;

        if(IsHindered) possibleMoves = new BattleTile[0];

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
                if (!aimedTiles.Contains(currentSkillTile)) continue;

                int moveGrade = GetMoveGrade(currentMoveTile.TileCoordinates, currentSkillTile.TileCoordinates, depth);

                if (moveGrade < bestGrade) continue;

                bestGrade = moveGrade;
                pickedMoveTile = possibleMoves[i];
                pickedSkillTile = currentSkillTile;
            }
            else
            {
                BattleTile[] possibleSkillTiles = 
                    BattleManager.Instance.GetPaternTiles(possibleMoves[i].TileCoordinates, currentSkillData.skillPatern, 
                    (int)Mathf.Sqrt(currentSkillData.skillPatern.Length), true).ToArray();

                for (int j = 0; j < possibleSkillTiles.Length; j++)
                {
                    int moveGrade = GetMoveGrade(possibleMoves[i].TileCoordinates, possibleSkillTiles[j].TileCoordinates, depth);
                    if (moveGrade > bestGrade)
                    {
                        bestGrade = moveGrade;
                        pickedMoveTile = possibleMoves[i];

                        if (possibleSkillTiles[j].UnitOnTile is null) continue;
                        if (!aimedTiles.Contains(possibleSkillTiles[j])) continue;

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

            switch (AIData.AI)
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

    #endregion


    #region Overrides

    protected override async void ClickUnit()
    {
        await Task.Yield();
        if (InputManager.wantsToRightClick) return;

        int newPreviewIndex = ((int)currentPreviewType + 1) % 3;
        currentPreviewType = (PreviewType)newPreviewIndex;

        CurrentTile.ClickTile();
        CurrentTile.OverlayTile();

        StartCoroutine(SquishCoroutine(0.15f));

        //base.ClickUnit();
    }

    protected override void Die()
    {
        StartCoroutine(DisappearCoroutine(1f));
    }

    #endregion
}
