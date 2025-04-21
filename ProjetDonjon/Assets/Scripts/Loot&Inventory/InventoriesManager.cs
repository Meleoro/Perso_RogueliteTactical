using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class InventoriesManager : GenericSingletonClass<InventoriesManager>
{
    [Header("Parameters")]
    [SerializeField] private float openEffectDuration;
    [SerializeField] private float closeEffectDuration;
    public float slotSize;

    [Header("Actions")]
    public Action OnInventoryOpen;
    public Action OnInventoryClose;

    [Header("Private Infos")]
    private Inventory[] heroesInventories = new Inventory[3];
    private List<InventorySlot> allSlots = new List<InventorySlot>();

    [Header("References")]
    [SerializeField] private RectTransform[] _hiddenInventoryPositions;
    [SerializeField] private RectTransform[] _shownInventoryPositions;
    [SerializeField] private RectTransform _mainLootParent;
    [SerializeField] private RectTransform[] _lootParents;
    public RectTransform MainLootParent { get { return _mainLootParent; } }
    [SerializeField] private RectTransform _inventoriesParent;
    [SerializeField] private Image _backInventoriesImage;
    [SerializeField] private Image _backFadeImage;


    private void Start()
    {
        _backInventoriesImage.rectTransform.localPosition = _hiddenInventoryPositions[1].localPosition;
    }

    public Inventory InitialiseInventory(Inventory inventoryPrefab, int index)
    {
        heroesInventories[index] = Instantiate(inventoryPrefab, _inventoriesParent);
        heroesInventories[index].SetupPosition(_hiddenInventoryPositions[index], _shownInventoryPositions[index], _lootParents[index]);
        heroesInventories[index].InitialiseInventory();

        allSlots.AddRange(heroesInventories[index].GetSlots());

        return heroesInventories[index];
    }


    #region Public Functions

    public void AddItem(Loot loot)
    {
        if (!VerifyCanOpenCloseInventory()) return;

        OpenInventories();
    }

    public InventorySlot VerifyIsOverlayingSlot(Vector3 raycastPos)
    {
        float maxDist = 0.5f;
        InventorySlot bestSlot = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < allSlots.Count; i++)
        {
            float currentDist = Vector2.Distance(raycastPos, allSlots[i]._rectTr.position);
            if (currentDist < maxDist)
            {
                if(currentDist < minDist)
                {
                    bestSlot = allSlots[i];
                    minDist = currentDist;
                }
            }
        }

        return bestSlot;
    }

    #endregion

    public bool VerifyCanOpenCloseInventory()
    {
        for(int i = 0; i < heroesInventories.Length; i++)
        {
            if (!heroesInventories[i].GetCanOpenOrClose())
                return false;
        }

        return true;
    }

    public async void OpenInventories()
    {
        OnInventoryOpen?.Invoke();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.75f, _shownInventoryPositions[1].localPosition + Vector3.up * 25f, CurveType.EaseOutSin);
        _backFadeImage.UFadeImage(openEffectDuration, 0.5f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < heroesInventories.Length; i++)
        {
            StartCoroutine(heroesInventories[i].OpenInventoryCoroutine(openEffectDuration));
        }

        await Task.Delay((int)(openEffectDuration * 0.75f * 1000));

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.25f, _shownInventoryPositions[1].localPosition, CurveType.EaseInSin);
    }

    public async void CloseInventories()
    {
        OnInventoryClose?.Invoke();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(closeEffectDuration, _hiddenInventoryPositions[1].localPosition, CurveType.EaseOutCubic);
        _backFadeImage.UFadeImage(closeEffectDuration, 0f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < heroesInventories.Length; i++)
        {
            StartCoroutine(heroesInventories[i].CloseInventoryCoroutine(closeEffectDuration));
        }
    }
}
