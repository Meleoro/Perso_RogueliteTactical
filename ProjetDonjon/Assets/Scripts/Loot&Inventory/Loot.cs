using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

[Serializable]
public class BackgroundType
{
    public Sprite backgroundSprite;
    public SpaceTakenRow[] spaceTaken;
}

public class Loot : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private LootData lootData;
    [SerializeField] private LayerMask UIcollisionLayer;
    [SerializeField] private BackgroundType[] possibleBackgrounds;
    [SerializeField] private Color[] lootColorAccordingToType;
    [SerializeField] private float dragLerpSpeed;

    [Header("Public Infos")]
    public LootData LootData { get { return lootData; } }

    [Header("Private Infos")]
    private bool isDragged;
    private bool isRotating;
    private bool isPlacedInInventory;
    private InventorySlot[] slotsOccupied;
    private EquipmentSlot overlayedEquipmentSlot;
    private Vector3 dragWantedPos;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Image _image;
    [SerializeField] private Image _imageBackground;
    [SerializeField] private Collider2D _collider;



    private void Start()
    {
        Initialise(lootData);
    }

    private void Update()
    {
        if (isPlacedInInventory) return;
        _imageBackground.rectTransform.position = Vector3.Lerp(_imageBackground.rectTransform.position, dragWantedPos, Time.deltaTime * dragLerpSpeed);

        float ratio = _imageBackground.rectTransform.rect.width / _imageBackground.rectTransform.rect.height;

        float difX = Mathf.Abs(dragWantedPos.x - _imageBackground.rectTransform.position.x) * ratio;
        float difY = Mathf.Abs(dragWantedPos.y - _imageBackground.rectTransform.position.y) * 0.5f;
        Vector3 addedSize = new Vector3(difX - difY * 0.1f, difY - difX * 0.5f) * 0.25f;

        _imageBackground.rectTransform.localScale = Vector3.one * 0.375f + addedSize;

        if (!isDragged) return;

        ManageDrag();
    }


    public void Initialise(LootData data)
    {
        lootData = data;
        _spriteRenderer.sprite = lootData.sprite;
        _image.sprite = lootData.sprite;

        _image.enabled = false;
    }


    private void BecomeInventoryItem()
    {
        _image.enabled = true;
        _imageBackground.enabled = true;
        _spriteRenderer.enabled = false;
        _collider.enabled = false;

        _imageBackground.rectTransform.position = CameraManager.Instance.transform.position + Vector3.forward * 9;
        _imageBackground.rectTransform.SetParent(InventoriesManager.Instance.MainLootParent);

        _image.rectTransform.localScale = Vector3.one;
        _image.SetNativeSize();

        _imageBackground.sprite = GetBackgroundSprite(lootData.spaceTaken);
        _imageBackground.rectTransform.localScale = Vector3.one * 0.4f;
        _imageBackground.SetNativeSize();

        _imageBackground.color = lootColorAccordingToType[(int)lootData.lootType];

        slotsOccupied = new InventorySlot[0];

        InventoriesManager.Instance.OnInventoryClose += BecomeWorldItem;
        isPlacedInInventory = false;
    }

    private void BecomeWorldItem()
    {
        InventoriesManager.Instance.OnInventoryClose -= BecomeWorldItem;

        for (int i = 0; i < slotsOccupied.Length; i++)
        {
            slotsOccupied[i].RemoveLoot();
        }

        _image.enabled = false;
        _imageBackground.enabled = false;
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }

    #region Equipment Functions

    public void EnterEquipmentSlot(EquipmentSlot equimentSlot)
    {
        overlayedEquipmentSlot = equimentSlot;
    }

    public void ExitEquipmentSlot()
    {
        overlayedEquipmentSlot = null;
    }

    #endregion


    #region UI Drag & Drop Functions

    public void Drag()
    {
        if (!isDragged)
        {
            isDragged = true;
            UIManager.Instance.DraggedLoot = this;
            ActualiseOverlayedSlots();

            _imageBackground.raycastTarget = false;

            if (isPlacedInInventory)
            {
                isPlacedInInventory = false;
                InventoriesManager.Instance.OnInventoryClose += BecomeWorldItem;
            }
        }

        Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        dragWantedPos = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);
    }

    public void Drop()
    {
        isDragged = false;
        UIManager.Instance.DraggedLoot = null;
        _imageBackground.raycastTarget = true;

        if (isRotating) return;

        // If we dropped the loot on an equipment slot
        if (overlayedEquipmentSlot is not null) 
        {
            overlayedEquipmentSlot.AddEquipment(this, true); 
        }

        List<InventorySlot> overlayedSlots = GetOverlayedSlots();

        // We check if the slots are empty
        int emptySlotsCount = GetOverlayedSlotsEmptyCount(overlayedSlots);
        if (emptySlotsCount != overlayedSlots.Count && overlayedEquipmentSlot is null) return;
        if (GetNeededOccupiedSpace() != emptySlotsCount && overlayedEquipmentSlot is null) return;

        // If no slots are selected
        if((overlayedSlots.Count == 0 && slotsOccupied.Length != 0) || overlayedEquipmentSlot is not null)
        {
            overlayedSlots = slotsOccupied.ToList();
        }
        else if(overlayedSlots.Count == 0 && slotsOccupied.Length == 0)
        {
            return;
        }

        // We remove the loot from the previous tiles
        for (int i = 0; i < slotsOccupied.Length; i++)
        {
            slotsOccupied[i].RemoveLoot();
        }

        SetupObjectPosition(overlayedSlots);

        InventoriesManager.Instance.OnInventoryClose -= BecomeWorldItem;
        isPlacedInInventory = true;
        overlayedEquipmentSlot = null;

        _imageBackground.rectTransform.UBounceScale(0.05f, Vector3.one * 0.33f, 0.15f, Vector3.one * 0.38f, CurveType.EaseInOutSin);
    }

    private void SetupObjectPosition(List<InventorySlot> overlayedSlots)
    {
        Vector3 position = Vector2.zero;
        slotsOccupied = new InventorySlot[overlayedSlots.Count];

        for (int i = 0; i < overlayedSlots.Count; i++)
        {
            position += overlayedSlots[i]._rectTr.position;
            overlayedSlots[i].AddLoot(this, _imageBackground);
            slotsOccupied[i] = overlayedSlots[i];
        }

        position /= overlayedSlots.Count;
        _imageBackground.rectTransform.position = position;
        dragWantedPos = position;
    }

    #endregion


    #region Inventory Private Functions

    private void ManageDrag()
    {
        ActualiseOverlayedSlots();

        if (isRotating) return;
        if (InputManager.wantsToRotateLeft)
        {
            StartCoroutine(RotateLootCoroutine(0.1f, _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, -90), 
                _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, -115)));
        }
        if (InputManager.wantsToRotateRight)
        {
            StartCoroutine(RotateLootCoroutine(0.1f, _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, 90),
                _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, 115)));
        }
    }

    private IEnumerator RotateLootCoroutine(float duration, Quaternion finalRotation, Quaternion furtherFinalRotation)
    {
        _imageBackground.rectTransform.UChangeRotation(duration * 0.7f, furtherFinalRotation);
        isRotating = true;

        yield return new WaitForSeconds(duration * 0.7f);
        _imageBackground.rectTransform.UStopChangeRotation();
        _imageBackground.rectTransform.UChangeRotation(duration * 0.3f, finalRotation);

        yield return new WaitForSeconds(duration * 0.3f);

        isRotating = false;
    }

    private int GetOverlayedSlotsEmptyCount(List<InventorySlot> overlayedSlots)
    {
        int returnedValue = 0;
        foreach (InventorySlot slot in overlayedSlots)
        {
            if (!slot.VerifyHasLoot() || slotsOccupied.Contains(slot)) returnedValue++;
        }

        return returnedValue;
    }

    private void ActualiseOverlayedSlots()
    {
        List<InventorySlot> overlayedSlots = GetOverlayedSlots();
        foreach (InventorySlot slot in overlayedSlots)
        {
            slot.OverlaySlot();
        }
    }

    private List<InventorySlot> GetOverlayedSlots()
    {
        List<InventorySlot> returnedSlots = new List<InventorySlot>();
        Vector2 bottomLeft = new Vector2((lootData.spaceTaken[0].row.Length * InventoriesManager.Instance.slotSize) * 0.5f - InventoriesManager.Instance.slotSize * 0.5f,
            (lootData.spaceTaken.Length * InventoriesManager.Instance.slotSize) * 0.5f - InventoriesManager.Instance.slotSize * 0.5f);

        if (lootData.spaceTaken[0].row.Length == 1) bottomLeft.x = 0;
        if (lootData.spaceTaken.Length == 1) bottomLeft.y = 0;

        for (int y = 0; y < lootData.spaceTaken.Length; y++)
        {
            for (int x = 0; x < lootData.spaceTaken[y].row.Length; x++)
            {
                if (!lootData.spaceTaken[y].row[x]) continue;

                Vector2 raycastRelativePos = new Vector2(x * InventoriesManager.Instance.slotSize, y * InventoriesManager.Instance.slotSize) - bottomLeft;
                Vector3 raycastPos = _imageBackground.rectTransform.TransformPoint(raycastRelativePos);

                InventorySlot foundSlot = InventoriesManager.Instance.VerifyIsOverlayingSlot(raycastPos);

                if (foundSlot is not null)
                {
                    returnedSlots.Add(foundSlot);
                }
            }
        }

        return returnedSlots;
    }

    private int GetNeededOccupiedSpace()
    {
        int returnedValue = 0;

        for(int x = 0; x < lootData.spaceTaken.Length; x++)
        {
            for (int y = 0; y < lootData.spaceTaken[x].row.Length; y++)
            {
                if (lootData.spaceTaken[x].row[y]) returnedValue++;
            }
        }

        return returnedValue;
    }

    private Sprite GetBackgroundSprite(SpaceTakenRow[] spaceTaken)
    {
        foreach (BackgroundType possibleBackground in possibleBackgrounds)
        {
            bool found = true;

            for(int x = 0; x < spaceTaken.Length; x++)
            {
                if (possibleBackground.spaceTaken.Length <= x)
                {
                    found = false;
                    break;
                }

                for (int y = 0; y < spaceTaken[x].row.Length; y++)
                {
                    if (possibleBackground.spaceTaken[x].row.Length <= y)
                    {
                        found = false; 
                        break;
                    }

                    if (spaceTaken[x].row[y] != possibleBackground.spaceTaken[x].row[y]) found = false;
                }
            }

            if (found)
                return possibleBackground.backgroundSprite;
        }

        return null;
    }

    #endregion


    #region Interface Functions

    public void CanBePicked()
    {
        transform.UChangeScale(0.1f, Vector3.one * 1.1f);
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 1f, "_OutlineSize");
    }

    public void CannotBePicked()
    {
        transform.UChangeScale(0.1f, Vector3.one);
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        InventoriesManager.Instance.AddItem(this);
        BecomeInventoryItem();
    }

    #endregion
}
