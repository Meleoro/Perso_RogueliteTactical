using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
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
    private BattleTileState saveTileState;
    private Vector3 savePos;
    private Coroutine highlightCoroutine;
    private BattleTile[] highlightedTiles;
    private bool isHole;

    [Header("Public Infos")]
    public Unit UnitOnTile { get { return unitOnTile; } }
    public List<BattleTile> TileNeighbors { get { return tileNeighbors; } }
    public Vector2Int TileCoordinates { get { return tileCoordinates; } }
    public BattleTileState CurrentTileState { get { return currentTileState; } }
    public bool IsHole { get { return isHole; } }

    [Header("References")]
    [SerializeField] private SpriteRenderer _mainSpriteRenderer;
    [SerializeField] private SpriteRenderer _backSpriteRenderer;
    [SerializeField] private Button _tileButton;



    #region Setup

    public void SetupBattleTile(Vector2Int tileCoordinates, bool isHole)
    {
        this.tileCoordinates = tileCoordinates;

        _mainSpriteRenderer.color = baseTileColorOutline;
        _backSpriteRenderer.color = baseTileColorBack;

        currentColorOutline = baseTileColorOutline;
        currentColorBack = baseTileColorBack;

        savePos = transform.position;
        highlightedTiles = new BattleTile[0];

        tileNeighbors = new List<BattleTile>();

        this.isHole = isHole;
        if (isHole)
        {
            _tileButton.enabled = false;
        }
    }

    public void AddNeighbor(BattleTile tile)
    {
        tileNeighbors.Add(tile);
    }

    #endregion

    private void LateUpdate()
    {
        saveTileState = BattleTileState.None;
    }

    #region Hide / Show

    public IEnumerator HideTileCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        _mainSpriteRenderer.enabled = false;
        _backSpriteRenderer.enabled = false;
    }

    public IEnumerator ShowTileCoroutine(float delay)
    {
        if (isHole) yield break;

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
        StartCoroutine(ChangeStateEffect(moveTileColorOutline, moveTileColorBack, true, saveTileState == BattleTileState.Move));
    }

    public void DisplayPossibleAttackTile(bool doBounce)
    {
        if (currentTileState == BattleTileState.Attack) return;

        currentTileState = BattleTileState.Attack;
        StartCoroutine(ChangeStateEffect(attackTileColorOutline, attackTileColorBack, doBounce, saveTileState == BattleTileState.Attack));
    }

    public void DisplayDangerTile()
    {
        if (currentTileState == BattleTileState.Danger) return;

        if (unitOnTile is not null) unitOnTile.DisplaySkillOutline(false);

        currentTileState = BattleTileState.Danger;
        StartCoroutine(ChangeStateEffect(dangerTileColorOutline, dangerTileColorBack, false, saveTileState == BattleTileState.Danger));
    }

    public void DisplayNormalTile()
    {
        if (currentTileState == BattleTileState.None) return;
        if (currentTileState == BattleTileState.Danger && unitOnTile is not null) unitOnTile.HideOutline();

        saveTileState = currentTileState;
        currentTileState = BattleTileState.None;
        StartCoroutine(ChangeStateEffect(baseTileColorOutline, baseTileColorBack, false));
    }

    private IEnumerator ChangeStateEffect(Color outlineColor, Color backColor, bool doBounce = true, bool doInstant = false)
    {
        currentColorOutline = outlineColor;
        currentColorBack = backColor;

        StopHighlight();

        if (doInstant)
        {
            _mainSpriteRenderer.UStopSpriteRendererLerpColor();
            _backSpriteRenderer.UStopSpriteRendererLerpColor();

            _mainSpriteRenderer.color = outlineColor;
            _backSpriteRenderer.color = backColor;

            yield break;
        }

        _mainSpriteRenderer.ULerpColorSpriteRenderer(0.15f, outlineColor);
        _backSpriteRenderer.ULerpColorSpriteRenderer(0.15f, backColor);

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
        if (currentTileState == BattleTileState.Attack && BattleManager.Instance.CurrentActionType == MenuType.Skills)
        {
            BattleManager.Instance.DisplayDangerTiles(this, null);
        }
        else
        {
            // Displays a preview of the possible movement of the tile's current unit if it's possible
            if (unitOnTile is not null && BattleManager.Instance.CurrentUnit is not null && BattleManager.Instance.CurrentActionType != MenuType.Skills)
            {
                unitOnTile.DisplayOverlayOutline();
                if (unitOnTile.GetType() != typeof(Hero) && BattleManager.Instance.CurrentUnit.GetType() == typeof(Hero))
                {
                    BattleManager.Instance.DisplayPossibleTiles(unitOnTile as AIUnit);
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
                unitOnTile.HideOutline();
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

    public async void ClickTile()
    {
        await Task.Yield();

        if (InputManager.wantsToRightClick) return;

        switch (currentTileState)
        {
            case BattleTileState.Move:
                if (BattleManager.Instance.CurrentActionType != MenuType.Move) break;
                StartCoroutine(BattleManager.Instance.MoveUnitCoroutine(this, false));
                break;

            case BattleTileState.Danger:
                if (BattleManager.Instance.CurrentActionType != MenuType.Skills) break;
                StartCoroutine(BattleManager.Instance.UseSkillCoroutine(null));
                break;
        }
    }

    #endregion


    #region Other Effects

    public void HighlightMovePathTile()
    {
        highlightCoroutine = StartCoroutine(HighlightTile(moveTileColorOutline, moveTileColorBack, 0.3f));
    }


    public void HighlightSkillTile()
    {
        highlightCoroutine = StartCoroutine(HighlightTile(attackTileColorOutline, attackTileColorBack, 0.3f));
    }


    public void StopHighlight()
    {
        if (highlightCoroutine is null) return;
        StopCoroutine(highlightCoroutine);

        _mainSpriteRenderer.UStopSpriteRendererLerpColor();
        _backSpriteRenderer.UStopSpriteRendererLerpColor();

        //_mainSpriteRenderer.ULerpColorSpriteRenderer(0.1f, currentColorOutline, CurveType.EaseInOutCubic);
        //_backSpriteRenderer.ULerpColorSpriteRenderer(0.1f, currentColorBack, CurveType.EaseInOutCubic);
    }


    private IEnumerator HighlightTile(Color outlineColor, Color backColor, float duration)
    {
        while (true)
        {
            _mainSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.98f, outlineColor + new Color(0.02f, 0.02f, 0.02f, 0.2f), CurveType.EaseInOutCubic);
            _backSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.98f, backColor + new Color(0.005f, 0.005f, 0.005f, 0.2f), CurveType.EaseInOutCubic);

            yield return new WaitForSeconds(duration);


            _mainSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.98f, outlineColor + new Color(0.01f, 0.01f, 0.01f, 0.1f), CurveType.EaseInOutCubic);
            _backSpriteRenderer.ULerpColorSpriteRenderer(duration * 0.98f, backColor + new Color(0.005f, 0.005f, 0.005f, 0.1f), CurveType.EaseInOutCubic);

            yield return new WaitForSeconds(duration);
        }
    }

    #endregion
}
