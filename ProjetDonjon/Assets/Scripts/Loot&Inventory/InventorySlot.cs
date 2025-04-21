using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float overlaySize;
    [SerializeField] private Sprite[] possibleSprites;

    [Header("Private Infos")]
    public List<InventorySlot> neighbors = new();
    public Vector2Int slotCoordinates;
    private Vector3 baseSize;
    private bool isOverlayed;
    private int overlayCount;
    private Loot attachedLoot;


    [Header("Public Infos")]
    public Vector2Int SlotCoordinates { get { return slotCoordinates; } }

    [Header("References")]
    public RectTransform _rectTr;
    public RectTransform _lootParent;
    public Image _mainSpriteRenderer;


    private void Start()
    {
        baseSize = _rectTr.localScale;

        _mainSpriteRenderer.sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
    }

    public void SetupCoordinates(Vector2Int coordinates)
    {
        slotCoordinates = coordinates;
    }


    #region Manage Slot Content Functions

    public bool VerifyHasLoot()
    {
        return attachedLoot != null;
    }

    public void AddLoot(Loot loot, Image image)
    {
        attachedLoot = loot;
        image.rectTransform.SetParent(_lootParent);
    }

    public Loot RemoveLoot()
    {
        Loot returnedLoot = attachedLoot;
        attachedLoot = null;
        return returnedLoot;
    }


    #endregion


    #region Neighbors Functions

    public void SetupNeighbors(InventorySlot[] allSlots, float maxDistBetweenTiles)
    {
        for(int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] == this) continue;

            float dist = Vector2.Distance(_rectTr.localPosition, allSlots[i]._rectTr.localPosition);
            if(dist <= maxDistBetweenTiles)
            {
                neighbors.Add(allSlots[i]);
            }
        }
    }

    public bool VerifyHasNeighbor(Vector2Int addedCoordinates)
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i].SlotCoordinates == slotCoordinates + addedCoordinates) 
                return true;
        }

        return false;
    }

    #endregion


    #region Mouse Feedbacks

    private void LateUpdate()
    {
        if (!isOverlayed) return;

        if(overlayCount == 1)
        {
            QuitOverlaySlot();
        }
        overlayCount++;
    }

    public void OverlaySlot()
    {
        transform.localScale = Vector3.one * overlaySize;
        isOverlayed = true;
        overlayCount = 0;
    }

    public void QuitOverlaySlot()
    {
        transform.localScale = baseSize;
        isOverlayed = false;
    }

    #endregion
}
