using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private AIData aiEliteData;
    [SerializeField] private Loot lootPrefab;
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private bool isBoss;

    [Header("Private Infos")]
    private BattleTile[] aimedTiles;
    private BattleTile[] avoidedTiles;
    private SkillData currentSkillData;
    private PreviewType currentPreviewType;
    private int currentSkillIndex;
    private AIData currentData;
    private bool isElite;

    [Header("Public Infos")]
    public AIData AIData { get { return isElite ? aiEliteData : aiData; } }
    public SkillData CurrentSkillData { get { return currentSkillData; } }
    public PreviewType CurrentPreviewType { get { return currentPreviewType; } }
    public bool IsBoss { get { return isBoss; } }

    [Header("Enemy References")]
    [SerializeField] private ParticleSystem _eliteVFX;


    #region Setup

    public void Initialise(bool isElite)
    {
        this.isElite = isElite;
        if (isElite)
        {
            currentData = aiEliteData;
            _eliteVFX.Play();
        }
        else
        {
            currentData = aiData;

            _spriteRenderer.material.SetFloat("_EliteEffectStrength", 0);
            _spriteRenderer.material.SetFloat("_EliteEffectNoiseStrength", 0);
        }

        unitData = currentData;
        currentSkillData = currentData.skills[0];
        currentSkillIndex = 0;

        currentPreviewType = PreviewType.Move;

        InitialiseUnitInfos(currentData.baseHealth, currentData.baseStrength, currentData.baseSpeed, currentData.baseLuck, 0);
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
                    if (BattleManager.Instance.CurrentEnemies[i].CurrentTile == currentTile) continue;
                    aimedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
                }
            }

            else if (currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Empty)
            {
                aimedTiles = BattleManager.Instance.BattleRoom.BattleTiles.ToArray();
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

            else if(currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Empty)
            {
                aimedTiles = BattleManager.Instance.BattleRoom.BattleTiles.ToArray();
            }

            else
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                    if (BattleManager.Instance.CurrentEnemies[i].CurrentTile == currentTile) continue;
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
        _spriteRenderer.material.SetFloat("_DitherProgress", -2);

        _spriteRenderer.material.ULerpMaterialFloat(duration, 3.5f, "_DitherProgress");

        yield return new WaitForSeconds(duration);

        _ui.ShowUnitUI();
    }

    private void SpawnLoot()
    {
        PossibleLootData[] possibleLoots =
            ProceduralGenerationManager.Instance.enviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].battleEndPossibleLoots;

        int pickedPercentage = Random.Range(0, 100);
        int currentSum = 0;

        for (int i = 0; i < possibleLoots.Length; i++)
        {
            currentSum += possibleLoots[i].probability;

            if (currentSum > pickedPercentage)
            {
                if (possibleLoots[i].loot is null) break;

                Loot newLoot = Instantiate(lootPrefab, transform.position, Quaternion.Euler(0, 0, 0));
                newLoot.Initialise(possibleLoots[i].loot);

                break;
            }
        }
    }

    private IEnumerator LastEnemyDisappearCoroutine(float duration)
    {
        CameraManager.Instance.FocusOnTr(transform, 3f);
        transform.UShakePosition(duration * 0.75f, 0.2f, 0.03f);

        BattleManager.Instance.StartBattleEndCutscene();

        yield return new WaitForSeconds(duration * 0.75f);

        // loot generation
        SpawnLoot();
        if (isBoss)
        {
            SpawnLoot();
            SpawnLoot();
        }

        int pickedCoinsAmount = Random.Range(ProceduralGenerationManager.Instance.enviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].minBattleCoins,
            ProceduralGenerationManager.Instance.enviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].maxBattleCoins);

        for (int i = 0; i < pickedCoinsAmount; i++)
        {
            Coin coin = Instantiate(coinPrefab, transform.position, Quaternion.Euler(0, 0, 0), UIManager.Instance.CoinUI.transform);
            coin.transform.position = transform.position;
        }

        StartCoroutine(DisappearCoroutine(duration * 0.25f));
    }

    private IEnumerator DisappearCoroutine(float duration)
    {
        BattleManager.Instance.RemoveUnit(this);
        _spriteRenderer.material.ULerpMaterialFloat(duration, -0.5f, "_DitherProgress");

        yield return new WaitForSeconds(duration);

        Destroy(gameObject);
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

            if(currentData.skills.Length > 1)
            {
                StartCoroutine(_ui.DoChangePaternEffectCoroutine(1.5f));
                currentSkillIndex = (currentSkillIndex + 1) % currentData.skills.Length;
                currentSkillData = currentData.skills[currentSkillIndex];

                yield return new WaitForSeconds(1.5f);
            }

            yield return new WaitForSeconds(0.5f);
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

            // If we can move
            if(depth < maxDepth)
            {
                BattleTile currentMoveTile = null;
                BattleTile currentSkillTile = null;

                (currentMoveTile, currentSkillTile) = GetBestMove(possibleMoves[i], depth+1, maxDepth);

                // If we didn't find any usable skill tile we skip
                if (currentSkillTile is null) continue;
                if (currentSkillTile.UnitOnTile is null) continue;
                if (currentSkillTile.UnitOnTile == this && currentSkillData.skillType != SkillType.SkillArea) continue;
                if (!aimedTiles.Contains(currentSkillTile) && currentSkillData.skillType != SkillType.SkillArea) continue;

                // If we can hit a target, we verify if the move has a good enough grade to be selected
                BattleTile[] dangerTiles = GetDangerTiles(currentMoveTile.TileCoordinates, currentSkillTile.TileCoordinates);
                int moveGrade = GetMoveGrade(currentMoveTile.TileCoordinates, dangerTiles, depth);
                if (moveGrade < bestGrade) continue;

                bestGrade = moveGrade;
                pickedMoveTile = possibleMoves[i];
                pickedSkillTile = currentSkillTile;
            }

            // If we need to test skills
            else 
            {
                BattleTile[] possibleSkillTiles;

                if (currentSkillData.skillType == SkillType.SkillArea) possibleSkillTiles = new BattleTile[1] { possibleMoves[i] };
                else possibleSkillTiles = BattleManager.Instance.GetPaternTiles(possibleMoves[i].TileCoordinates, currentSkillData.skillPatern,
                        (int)Mathf.Sqrt(currentSkillData.skillPatern.Length), true, false, false, currentTile).ToArray();

                for (int j = 0; j < possibleSkillTiles.Length; j++)
                {
                    BattleTile[] dangerTiles = GetDangerTiles(possibleMoves[i].TileCoordinates, possibleSkillTiles[j].TileCoordinates);
                    int moveGrade = GetMoveGrade(possibleMoves[i].TileCoordinates, dangerTiles, depth);

                    if (moveGrade > bestGrade)
                    {
                        bestGrade = moveGrade;
                        pickedMoveTile = possibleMoves[i];

                        foreach (var dangerTile in dangerTiles)
                        {
                            if (dangerTile.UnitOnTile is null) continue;
                            if (!aimedTiles.Contains(dangerTile)) continue;

                            pickedSkillTile = possibleSkillTiles[j];
                            break;
                        }
                    }
                }
            }
        }

        return (pickedMoveTile, pickedSkillTile);
    }

    private BattleTile[] GetDangerTiles(Vector2Int movePos, Vector2Int skillPos)
    {
        // Skill Grade
        BattleTile[] skillDangerTiles;
        if (currentSkillData.skillType == SkillType.SkillArea)
        {
            skillDangerTiles = BattleManager.Instance.GetPaternTiles(movePos,
                currentSkillData.skillPatern, (int)Mathf.Sqrt(currentSkillData.skillPatern.Length)).ToArray();
        }
        else if (currentSkillData.useOrientatedAOE)
        {
            Vector2Int coordinateDif = skillPos - CurrentTile.TileCoordinates;

            if (coordinateDif.y != 0)
                skillDangerTiles = BattleManager.Instance.GetPaternTiles(skillPos, currentSkillData.skillAOEPaternVertical,
                    (int)Mathf.Sqrt(currentSkillData.skillAOEPaternVertical.Length), false).ToArray();

            else
                skillDangerTiles = BattleManager.Instance.GetPaternTiles(skillPos, currentSkillData.skillAOEPaternHorizontal,
                    (int)Mathf.Sqrt(currentSkillData.skillAOEPaternHorizontal.Length), false).ToArray();
        }
        else
        {
            skillDangerTiles = BattleManager.Instance.GetPaternTiles(skillPos,
                currentSkillData.skillAOEPatern, (int)Mathf.Sqrt(currentSkillData.skillAOEPatern.Length)).ToArray();
        }

        return skillDangerTiles;
    }

    private int GetMoveGrade(Vector2Int testedMovePos, BattleTile[] testedDangerTiles, int depth)
    {
        int finalGrade = -5 * depth;

        // Move grade
        for (int i = 0; i < avoidedTiles.Length; i++)
        {
            int currentDist = (int)Vector2Int.Distance(testedMovePos, avoidedTiles[i].TileCoordinates);

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
        for (int i = 0; i < testedDangerTiles.Length; i++)
        {
            if (!aimedTiles.Contains(testedDangerTiles[i])) continue;

            finalGrade += 10;
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
        currentTile.UnitLeaveTile();

        if(BattleManager.Instance.CurrentEnemies.Count == 1 || isBoss)
        {
            StartCoroutine(LastEnemyDisappearCoroutine(2f));
        }
        else
        {
            StartCoroutine(DisappearCoroutine(1f));
        }
    }

    #endregion
}
