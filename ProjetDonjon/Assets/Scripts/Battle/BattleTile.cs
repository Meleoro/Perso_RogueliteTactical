using DG.Tweening;
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
    private Coroutine changeStateEffectCoroutine;
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

        if (changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(moveTileColorOutline, moveTileColorBack, true, saveTileState == BattleTileState.Move));
    }

    public void DisplayPossibleAttackTile(bool doBounce)
    {
        if (currentTileState == BattleTileState.Attack) return;
        currentTileState = BattleTileState.Attack;

        if (changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(attackTileColorOutline, attackTileColorBack, doBounce, saveTileState == BattleTileState.Attack));
    }

    public void DisplayDangerTile()
    {
        if (currentTileState == BattleTileState.Danger) return;
        if (unitOnTile is not null) unitOnTile.DisplaySkillOutline(false);
        currentTileState = BattleTileState.Danger;

        if (changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(dangerTileColorOutline, dangerTileColorBack, false, saveTileState == BattleTileState.Danger));
    }

    public void DisplayNormalTile()
    {
        if (currentTileState == BattleTileState.None) return;
        if (currentTileState == BattleTileState.Danger && unitOnTile is not null) unitOnTile.HideOutline();
        saveTileState = currentTileState;
        currentTileState = BattleTileState.None;

        if(changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(baseTileColorOutline, baseTileColorBack, false));
    }

    private IEnumerator ChangeStateEffect(Color outlineColor, Color backColor, bool doBounce = true, bool doInstant = false)
    {
        currentColorOutline = outlineColor;
        currentColorBack = backColor;

        StopHighlight();

        if (doInstant)
        {
            _mainSpriteRenderer.DOComplete();
            transform.DOComplete();

            _mainSpriteRenderer.color = outlineColor;
            _backSpriteRenderer.color = backColor;

            yield break;
        }

        if (doBounce)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.05f));

            float randomDelay = 0.1f + Random.Range(-0.03f, 0.03f);

            _mainSpriteRenderer.DOColor(outlineColor + Color.white * 0.25f, randomDelay).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor + Color.white * 0.25f, randomDelay).SetEase(Ease.InOutCubic);

            transform.DOMove(savePos + new Vector3(0, 0.1f, 0), randomDelay).SetEase(Ease.InOutCubic);
            transform.DOScale(new Vector3(1, Random.Range(1f, 1.15f), 1), randomDelay).SetEase(Ease.InOutCubic);

            yield return new WaitForSeconds(randomDelay);

            transform.DOMove(savePos, randomDelay).SetEase(Ease.InOutCubic);
            transform.DOScale(Vector3.one, randomDelay).SetEase(Ease.InOutCubic);

            _mainSpriteRenderer.DOColor(outlineColor, randomDelay).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor, randomDelay).SetEase(Ease.InOutCubic);
        }
        else
        {
            _mainSpriteRenderer.DOColor(outlineColor, 0.15f).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor, 0.15f).SetEase(Ease.InOutCubic);

            transform.DOMove(savePos, 0.2f).SetEase(Ease.InOutCubic);
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutCubic);
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

            BattleManager.Instance.PathCalculator.ActualisePathCalculatorTiles(BattleManager.Instance.BattleRoom.PlacedBattleTiles);
            Vector2Int[] path = BattleManager.Instance.PathCalculator.GetPath(BattleManager.Instance.CurrentUnit.CurrentTile.TileCoordinates, TileCoordinates, false).ToArray();
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
        await Task.Delay((int)(Time.deltaTime * 1000));

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

        _mainSpriteRenderer.color = currentColorOutline;
        _backSpriteRenderer.color = currentColorBack;
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
