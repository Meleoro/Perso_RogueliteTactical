using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class Inventory : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float distanceBetweenSlots;
    [SerializeField] private bool isChest;

    [Header("Public Infos")]
    public RectTransform RectTransform { get { return _rectTransform; } }
    public RectTransform LootParent { get { return _lootParent; } }
    public Hero AssociatedHero { get { return associatedHero; } }
    public InventorySlot[,] InventorySlotsTab { get { return inventorySlotsTab; } }
    public bool IsChest { get { return isChest; } }

    [Header("Private Infos")]
    private InventorySlot[,] inventorySlotsTab = new InventorySlot[0, 0];
    private InventorySlot[] inventorySlots;
    private InventorySlot[] upgradeSlots1;
    private InventorySlot[] upgradeSlots2;
    private RectTransform originalParent;
    private RectTransform originalLootParentParent;   // Oui
    private Hero associatedHero;
    private bool isMoving;

    [Header("References")]
    [SerializeField] private RectTransform _slotsParent;
    [SerializeField] private RectTransform _overlayedSlotsParent;
    [SerializeField] private RectTransform _upgrade1SlotsParent;
    [SerializeField] private RectTransform _upgrade2SlotsParent;
    private RectTransform _rectTransform;
    private RectTransform _hiddenRectTr;
    private RectTransform _showedRectTr;
    private RectTransform _lootParent;



    #region Initialisation Functions

    public void SetupPosition(RectTransform hiddenPosRectTr, RectTransform showedPosRectTr, RectTransform lootParent)
    {
        _hiddenRectTr = hiddenPosRectTr;
        _showedRectTr = showedPosRectTr;
        _lootParent = lootParent;

        _rectTransform = GetComponent<RectTransform>();

        if(_hiddenRectTr is not null)
            _rectTransform.position = _hiddenRectTr.position;

        originalParent = _rectTransform.parent as RectTransform;
        originalLootParentParent = _lootParent.parent as RectTransform;
    }

    public void InitialiseInventory(Hero hero)
    {
        associatedHero = hero;

        inventorySlots = _slotsParent.GetComponentsInChildren<InventorySlot>();
        if (!isChest)
        {
            upgradeSlots1 = _upgrade1SlotsParent.GetComponentsInChildren<InventorySlot>();
            upgradeSlots2 = _upgrade2SlotsParent.GetComponentsInChildren<InventorySlot>();
        }
        SetupSlots();

        if (isChest) InventoriesManager.Instance.AddSlots(inventorySlots);

        Vector2Int inventoryDimensions = GetInventoryDimensions();
        inventorySlotsTab = new InventorySlot[inventoryDimensions.x + 1, inventoryDimensions.y + 1];

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlotsTab[inventorySlots[i].SlotCoordinates.x, inventorySlots[i].SlotCoordinates.y] = inventorySlots[i];
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetupReferences(_lootParent, _slotsParent, _overlayedSlotsParent, this);
        }

        if (isChest) return;

        for (int i = 0; i < upgradeSlots1.Length; i++)
        {
            inventorySlots[i].SetupReferences(_lootParent, _slotsParent, _overlayedSlotsParent, this);
            upgradeSlots1[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < upgradeSlots2.Length; i++)
        {
            inventorySlots[i].SetupReferences(_lootParent, _slotsParent, _overlayedSlotsParent, this);
            upgradeSlots2[i].gameObject.SetActive(false);
        }
    }


    private void SetupSlots()
    {
        Vector2 bottomLeftPosition = inventorySlots[0].RectTransform.localPosition;

        // Neighbors + Find bottom left location
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetupNeighbors(inventorySlots, distanceBetweenSlots);

            if (inventorySlots[i].RectTransform.localPosition.x < bottomLeftPosition.x)
                bottomLeftPosition.x = inventorySlots[i].RectTransform.localPosition.x;

            if (inventorySlots[i].RectTransform.localPosition.y < bottomLeftPosition.y)
                bottomLeftPosition.y = inventorySlots[i].RectTransform.localPosition.y;
        }

        // Coordinates
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Vector2 slotOffset = (Vector2)inventorySlots[i].RectTransform.localPosition - bottomLeftPosition;
            Vector2Int slotCoordinates = new Vector2Int((int)(slotOffset.x / distanceBetweenSlots), (int)(slotOffset.y / distanceBetweenSlots));

            inventorySlots[i].SetupCoordinates(slotCoordinates);
        }
    }

    #endregion


    #region Open / Close Functions

    public bool GetCanOpenOrClose()
    {
        return !isMoving;
    }

    public IEnumerator OpenInventoryCoroutine(float duration, RectTransform forcedHiddenTr, RectTransform forcedShownTr)
    {
        _rectTransform.position = forcedHiddenTr.position;
        _lootParent.position = forcedHiddenTr.position;

        isMoving = true;

        _rectTransform.UChangePosition(duration * 0.75f, forcedShownTr.position + Vector3.up * 0.2f, CurveType.EaseOutSin);
        _lootParent.UChangePosition(duration * 0.75f, forcedShownTr.position + Vector3.up * 0.2f, CurveType.EaseOutSin);

        yield return new WaitForSeconds(duration * 0.75f + Time.deltaTime * 2);

        _rectTransform.UChangePosition(duration * 0.25f, forcedShownTr.position, CurveType.EaseInSin);
        _lootParent.UChangePosition(duration * 0.25f, forcedShownTr.position, CurveType.EaseInSin);

        yield return new WaitForSeconds(duration * 0.25f);

        isMoving = false;
    }

    public IEnumerator CloseInventoryCoroutine(float duration, RectTransform forcedHiddenTr)
    {
        isMoving = true;

        _rectTransform.UChangePosition(duration, forcedHiddenTr.position, CurveType.EaseOutCubic);
        _lootParent.UChangePosition(duration, forcedHiddenTr.position, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration);

        isMoving = false;
    }


    public IEnumerator OpenInventoryCoroutine(float duration)
    {
        _rectTransform.position = _hiddenRectTr.position;
        _lootParent.position = _hiddenRectTr.position;

        isMoving = true;
        HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.StopControl();

        _rectTransform.UChangeLocalPosition(duration * 0.75f, _rectTransform.parent.InverseTransformPoint(_showedRectTr.position + Vector3.up), CurveType.EaseOutSin);
        _lootParent.UChangeLocalPosition(duration * 0.75f, _rectTransform.parent.InverseTransformPoint(_showedRectTr.position + Vector3.up), CurveType.EaseOutSin);

        yield return new WaitForSeconds(duration * 0.75f + Time.deltaTime);

        _rectTransform.UChangeLocalPosition(duration * 0.25f, _rectTransform.parent.InverseTransformPoint(_showedRectTr.position), CurveType.EaseInSin);
        _lootParent.UChangeLocalPosition(duration * 0.25f, _rectTransform.parent.InverseTransformPoint(_showedRectTr.position), CurveType.EaseInSin);

        yield return new WaitForSeconds(duration * 0.25f);

        isMoving = false;
    }

    public IEnumerator CloseInventoryCoroutine(float duration)
    {
        isMoving = true;
        HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.RestartControl();

        _rectTransform.UChangeLocalPosition(duration, _rectTransform.parent.InverseTransformPoint(_hiddenRectTr.position), CurveType.EaseOutCubic);
        _lootParent.UChangeLocalPosition(duration, _rectTransform.parent.InverseTransformPoint(_hiddenRectTr.position), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration);

        isMoving = false; 
    }

    #endregion


    #region Others

    public InventorySlot[] GetSlots()
    {
        return inventorySlots;
    }

    private Vector2Int GetInventoryDimensions()
    {
        Vector2Int result = Vector2Int.zero;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].SlotCoordinates.x > result.x)
                result.x = inventorySlots[i].SlotCoordinates.x;

            if (inventorySlots[i].SlotCoordinates.y > result.y)
                result.y = inventorySlots[i].SlotCoordinates.y;
        }

        return result;
    }

    public void RebootPosition()
    {
        _rectTransform.SetParent(originalParent);
        _lootParent.SetParent(originalLootParentParent);

        _rectTransform.position = _hiddenRectTr.position;
        _lootParent.position = _hiddenRectTr.position;
    }

    #endregion


    public void AutoPlaceItem(Loot item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            List<InventorySlot> overlayedSlots = GetOverlayedCoordinates(inventorySlots[i].SlotCoordinates, item.LootData.spaceTaken, 0);
            if (overlayedSlots.Count == 0) continue;

            item.PlaceInInventory(overlayedSlots);
            return;
        }
    }

    public List<InventorySlot> GetOverlayedCoordinates(Vector2Int bottomLeftCoord, SpaceTakenRow[] spaceTaken, float currentAngle)
    {
        List<InventorySlot> validSlots = new List<InventorySlot>();

        for (int y = 0; y < spaceTaken.Length; y++)
        {
            for (int x = 0; x < spaceTaken[y].row.Length; x++)
            {
                Vector2 currentCoord = bottomLeftCoord;
                currentCoord += new Vector2(x, y).RotateDirection(currentAngle);

                InventorySlot currentSlot = GetSlotAtCoord(new Vector2Int(Mathf.RoundToInt(currentCoord.x), Mathf.RoundToInt(currentCoord.y)));

                if (currentSlot is null) return new List<InventorySlot>();    // If the placement isn't valid
                if (currentSlot.VerifyHasLoot()) return new List<InventorySlot>();
                validSlots.Add(currentSlot);             // If the placement is valid
            }
        }

        return validSlots;
    }

    private InventorySlot GetSlotAtCoord(Vector2Int coord)
    {
        if(coord.x < 0 || coord.y < 0) return null;
        if (coord.x >= inventorySlotsTab.GetLength(0) || coord.y >= inventorySlotsTab.GetLength(1)) return null;
        return inventorySlotsTab[coord.x, coord.y];
    }
}
