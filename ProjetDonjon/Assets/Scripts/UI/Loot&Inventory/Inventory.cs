using System.Collections;
using UnityEngine;
using Utilities;

public class Inventory : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float distanceBetweenSlots;

    [Header("Public Infos")]
    public RectTransform RectTransform { get { return _rectTransform; } }
    public RectTransform LootParent { get { return _lootParent; } }
    public Hero AssociatedHero { get { return associatedHero; } }

    [Header("Private Infos")]
    private InventorySlot[,] orderedInventorySlots = new InventorySlot[0, 0];
    private InventorySlot[] inventorySlots;
    private InventorySlot[] upgradeSlots1;
    private InventorySlot[] upgradeSlots2;
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
        _rectTransform.position = _hiddenRectTr.position;
        _lootParent.position = _hiddenRectTr.position;
    }


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


    #region Initialisation Functions

    public void SetupPosition(RectTransform hiddenPosRectTr, RectTransform showedPosRectTr, RectTransform lootParent)
    {
        _hiddenRectTr = hiddenPosRectTr;
        _showedRectTr = showedPosRectTr;
        _lootParent = lootParent;

        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.position = _hiddenRectTr.position;
    }

    public void InitialiseInventory(Hero hero)
    {
        associatedHero = hero;

        inventorySlots = _slotsParent.GetComponentsInChildren<InventorySlot>();
        upgradeSlots1 = _upgrade1SlotsParent.GetComponentsInChildren<InventorySlot>();
        upgradeSlots2 = _upgrade2SlotsParent.GetComponentsInChildren<InventorySlot>();
        SetupSlots();

        Vector2Int inventoryDimensions = GetInventoryDimensions();
        orderedInventorySlots = new InventorySlot[inventoryDimensions.x + 1, inventoryDimensions.y + 1];

        for(int i = 0; i < inventorySlots.Length; i++) 
        {
            orderedInventorySlots[inventorySlots[i].SlotCoordinates.x, inventorySlots[i].SlotCoordinates.y] = inventorySlots[i];
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetupReferences(_lootParent, _slotsParent, _overlayedSlotsParent, this);
        }

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

            if(inventorySlots[i].RectTransform.localPosition.x < bottomLeftPosition.x)
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
}
