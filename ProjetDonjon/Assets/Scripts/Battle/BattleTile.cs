using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utilities;

public enum BattleTileState
{
    None,
    Move,
    Attack,
    Danger
}

public class BattleTile : MonoBehaviour
{
    [Header("Base Parameters")]
    [SerializeField] private Color baseTileColorOutline;
    [SerializeField] private Color moveTileColorOutline;
    [SerializeField] private Color attackTileColorOutline;
    [SerializeField] private Color dangerTileColorOutline;
    [SerializeField] private Color baseTileColorBack;
    [SerializeField] private Color moveTileColorBack;
    [SerializeField] private Color attackTileColorBack;
    [SerializeField] private Color dangerTileColorBack;

    [Header("Overlay Parameters")]
    [SerializeField] private float overlayEffectDuration;
    [SerializeField] private Color addedColorOverlayOutline;
    [SerializeField] private Color addedColorOverlayBack;

    [Header("Private Infos")]
    private Vector2Int tileCoordinates;
    private Unit unitOnTile;
    private Color currentColorOutline;
    private Color currentColorBack;
    private List<BattleTile> tileNeighbors = new List<BattleTile>();
    private BattleTileState currentTileState;
    private Vector3 savePos;
    private Coroutine highlightCoroutine;
    private BattleTile[] highlightedTiles;

    [Header("Public Infos")]
    public Unit UnitOnTile { get { return unitOnTile; } }
    public List<BattleTile> TileNeighbors { get { return tileNeighbors; } }
    public Vector2Int TileCoordinates { get { return tileCoordinates; } }
    public BattleTileState CurrentTileState { get { return currentTileState; } }

    [Header("References")]
    [SerializeField] private SpriteRenderer _mainSpriteRenderer;
    [SerializeField] private SpriteRenderer _backSpriteRenderer;



    #region Setup


    public void SetupBattleTile(Vector2Int tileCoordinates)
    {
        this.tileCoordinates = tileCoordinates;

        _mainSpriteRenderer.color = baseTileColorOutline;
        _backSpriteRenderer.color = baseTileColorBack;

        currentColorOutline = baseTileColorOutline;
        currentColorBack = baseTileColorBack;

        savePos = transform.position;
        highlightedTiles = new BattleTile[0];

        tileNeighbors = new List<BattleTile>();
    }

    public void AddNeighbor(BattleTile tile)
    {
        tileNeighbors.Add(tile);
    }

    #endregion


    #region Hide / Show

    public IEnumerator HideTileCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        _mainSpriteRenderer.enabled = false;
        _backSpriteRenderer.enabled = false;
    }

    public IEnumerator ShowTileCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        _mainSpriteRenderer.enabled = true;
        _backSpriteRenderer.enabled = true;
    }

    #endregion


    #region Tile States 

    public void DisplayMoveTile()
    {
        if (currentTileState == BattleTileState.Move) return;

        currentTileState = BattleTileState.Move;

        StartCoroutine(ChangeStateEffect(moveTileColorOutline, moveTileColorBack));
    }

    public void DisplayPossibleAttackTile(bool doBounce)
    {
        if (currentTileState == BattleTileState.Attack) return;

        currentTileState = BattleTileState.Attack;

        StartCoroutine(ChangeStateEffect(attackTileColorOutline, attackTileColorBack, doBounce));
    }

    public void DisplayDangerTile()
    {
        if (currentTileState == BattleTileState.Danger) return;

        currentTileState = BattleTileState.Danger;

        StartCoroutine(ChangeStateEffect(dangerTileColorOutline, dangerTileColorBack, false));
    }

    public void DisplayNormalTile()
    {
        if (currentTileState == BattleTileState.None) return;

        currentTileState = BattleTileState.None;

        StartCoroutine(ChangeStateEffect(baseTileColorOutline, baseTileColorBack, false));
    }

    private IEnumerator ChangeStateEffect(Color outlineColor, Color backColor, bool doBounce = true)
    {
        StopHighlight();

        _mainSpriteRenderer.ULerpColorSpriteRenderer(0.15f, outlineColor);
        _backSpriteRenderer.ULerpColorSpriteRenderer(0.15f, backColor);

        currentColorOutline = outlineColor;
        currentColorBack = backColor;

        if (doBounce)
        {
            transform.UChangePosition(0.05f, savePos + new Vector3(0, 0.08f, 0), CurveType.EaseInOutCubic);

            yield return new WaitForSeconds(0.05f);

            transform.UChangePosition(0.1f, savePos, CurveType.EaseInOutCubic);
        }
    }

    #endregion


    #region Tile Content 

    public void UnitEnterTile(Unit unit)
    {
        unitOnTile = unit;
    }

    public void UnitLeaveTile()
    {
        unitOnTile = null;
    }

    #endregion


    #region Mouse Input Functions

    public void OverlayTile()
    {
        if (currentTileState == BattleTileState.Attack)
        {
            BattleManager.Instance.DisplayDangerTiles(this, null);
        }
        else
        {
            // Displays a preview of the possible movement of the tile's current unit if it's possible
            if (unitOnTile is not null && BattleManager.Instance.CurrentUnit is not null && BattleManager.Instance.CurrentActionType != MenuType.Skills)
            {
                if (unitOnTile.GetType() != typeof(Hero) && BattleManager.Instance.CurrentUnit.GetType() == typeof(Hero))
                {
                    BattleManager.Instance.DisplayPossibleMoveTiles(unitOnTile as Enemy);
                }
            }
        }

        _mainSpriteRenderer.color = currentColorOutline + addedColorOverlayOutline;
        _backSpriteRenderer.color = currentColorBack + addedColorOverlayBack;

        // We display the path to reach this tile
        if(currentTileState == BattleTileState.Move)
        {
            if (BattleManager.Instance.CurrentUnit.CurrentTile.TileCoordinates == TileCoordinates) return;

            BattleManager.Instance._pathCalculator.ActualisePathCalculatorTiles(BattleManager.Instance.BattleRoom.PlacedBattleTiles);
            Vector2Int[] path = BattleManager.Instance._pathCalculator.GetPath(BattleManager.Instance.CurrentUnit.CurrentTile.TileCoordinates, TileCoordinates, false).ToArray();
            if (path.Length <= 1) return;
            highlightedTiles = new BattleTile[path.Length - 1];

            for(int i = 0; i < path.Length - 1; i++)
            {
                BattleManager.Instance.BattleRoom.PlacedBattleTiles[path[i].x, path[i].y].HighlightMovePathTile();
                highlightedTiles[i] = BattleManager.Instance.BattleRoom.PlacedBattleTiles[path[i].x, path[i].y];
            }
        }
    }

    public void QuitOverlayTile()
    {
        if (currentTileState == BattleTileState.Danger)
        {
            BattleManager.Instance.DisplayPossibleSkillTiles(null, null, false);
        }
        else
        {
            // Hides the preview of the possible movement of the tile's current unit if it's possible
            if (unitOnTile is not null && BattleManager.Instance.CurrentUnit is not null)
            {
                if (unitOnTile.GetType() != typeof(Hero) && BattleManager.Instance.CurrentUnit.GetType() == typeof(Hero))
                {
                    BattleManager.Instance.StopDisplayPossibleMoveTiles();
                }
            }
        }

        if(highlightedTiles.Length > 0)
        {
            for (int i = 0; i < highlightedTiles.Length; i++)
            {
                highlightedTiles[i].StopHighlight();
            }

            highlightedTiles = new BattleTile[0];
        }

        _mainSpriteRenderer.color = currentColorOutline;
        _backSpriteRenderer.color = currentColorBack;
    }

    public void ClickTile()
    {
        switch (currentTileState)
        {
            case BattleTileState.Move:
                StartCoroutine(BattleManager.Instance.MoveUnitCoroutine(this, false));
                break;

            case BattleTileState.Danger:
                StartCoroutine(BattleManager.Instance.UseSkillCoroutine(null));
                break;
        }
    }

    #endregion


    #region Other Effects

    public void HighlightMovePathTile()
    {
        highlightCoroutine = StartCoroutine(HighlightTile(moveTileColorOutline, moveTileColorBack, 0.4f));
    }


    public void HighlightSkillTile()
    {
        highlightCoroutine = StartCoroutine(HighlightTile(attackTileColorOutline, attackTileColorBack, 0.4f));
    }


    public void StopHighlight()
    {
        if (highlightCoroutine is null) return;
        StopCoroutine(highlightCoroutine);

        _mainSpriteRenderer.ULerpColorSpriteRenderer(0.1f, currentColorOutline, CurveType.EaseInOutCubic);
        _backSpriteRenderer.ULerpColorSpriteRenderer(0.1f, currentColorBack, CurveType.EaseInOutCubic);
    }


    private IEnumerator HighlightTile(Color outlineColor, Color backColor, float duration)
    {
        while (true)
        {
            _mainSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.95f, outlineColor + new Color(0.02f, 0.02f, 0.02f, 0.2f), CurveType.EaseInOutCubic);
            _backSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.95f, backColor + new Color(0.005f, 0.005f, 0.005f, 0.2f), CurveType.EaseInOutCubic);

            yield return new WaitForSeconds(duration);


            _mainSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.95f, outlineColor, CurveType.EaseInOutCubic);
            _backSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.95f, backColor, CurveType.EaseInOutCubic);

            yield return new WaitForSeconds(duration);
        }
    }

    #endregion
}
