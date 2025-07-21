using System;
using System.Collections;
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
    [SerializeField] private Sprite[] inventoryBackSprites;

    [Header("Actions")]
    public Action OnInventoryOpen;
    public Action OnInventoryClose;

    [Header("Private Infos")]
    private Inventory[] heroesInventories = new Inventory[3];
    private List<InventorySlot> allSlots = new List<InventorySlot>();
    private int inventoryInstantiatedAmount = 0;

    [Header("Public Infos")]
    public RectTransform MainLootParent { get { return _mainLootParent; } }
    public DetailsPanel DetailsPanel { get { return _detailsPanel; } }
    public InventoryActionPanel InventoryActionPanel { get { return _inventoryActionsPanel; } }  


    [Header("References")]
    [SerializeField] private RectTransform[] _hiddenInventoryPositions;
    [SerializeField] private RectTransform[] _shownInventoryPositions;
    [SerializeField] private RectTransform _mainLootParent;
    [SerializeField] private RectTransform _hiddenPosition;
    [SerializeField] private RectTransform _shownPosition;
    [SerializeField] private RectTransform _leftHiddenPosition;
    [SerializeField] private RectTransform _leftShownPosition;
    [SerializeField] private RectTransform[] _lootParents;
    [SerializeField] private RectTransform _inventoriesParent;
    [SerializeField] private Image _backInventoriesImage;
    [SerializeField] private Image _backFadeImage;
    [SerializeField] private DetailsPanel _detailsPanel;
    [SerializeField] private InventoryActionPanel _inventoryActionsPanel;


    private void Start()
    {
        _backInventoriesImage.rectTransform.position = _hiddenPosition.position;
    }

    public Inventory InitialiseInventory(Inventory inventoryPrefab, int index, Hero hero)
    {
        heroesInventories[index] = Instantiate(inventoryPrefab, _inventoriesParent);
        heroesInventories[index].SetupPosition(_hiddenInventoryPositions[index], _shownInventoryPositions[index], _lootParents[index]);
        heroesInventories[index].InitialiseInventory(hero);

        _backInventoriesImage.sprite = inventoryBackSprites[index];
        _backInventoriesImage.SetNativeSize();

        allSlots.AddRange(heroesInventories[index].GetSlots());
        inventoryInstantiatedAmount++;

        _hiddenInventoryPositions[index].gameObject.SetActive(true);
        _shownInventoryPositions[index].gameObject.SetActive(true);

        StartCoroutine(ResetPosDelayCoroutine());

        return heroesInventories[index];
    }

    private IEnumerator ResetPosDelayCoroutine() 
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            heroesInventories[i].RebootPosition(); 
        }
    }


    public void AddItem(Loot loot)
    {
        if (!VerifyCanOpenCloseInventory()) return;

        OpenInventories();
    }

    public InventorySlot VerifyIsOverlayingSlot(Vector3 raycastPos, Vector3 centerPos)
    {
        float maxDist = 75f;
        InventorySlot bestSlot = null;
        float minDist1 = Mathf.Infinity;
        List<InventorySlot> possibleSlots = new List<InventorySlot>();

        for (int i = 0; i < allSlots.Count; i++)
        {
            float currentDist = Vector2.Distance(allSlots[i].RectTransform.InverseTransformPoint(raycastPos), Vector3.zero);
            if (currentDist < maxDist)
            {
                currentDist += Vector2.Distance(allSlots[i].RectTransform.InverseTransformPoint(centerPos), Vector3.zero);
                possibleSlots.Add(allSlots[i]);

                if(currentDist < minDist1)
                {
                    bestSlot = allSlots[i];
                    minDist1 = currentDist;  
                }
            }
        }

        return bestSlot;
    }


    #region Open / Close Functions


    public bool VerifyCanOpenCloseInventory()
    {
        for(int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            if (!heroesInventories[i].GetCanOpenOrClose())
                return false;
        }

        return true;
    }


    public IEnumerator OpenInventory(Inventory inventoryToOpen)
    {
        StartCoroutine(inventoryToOpen.OpenInventoryCoroutine(openEffectDuration, _leftHiddenPosition, _leftShownPosition));

        yield return new WaitForSeconds(openEffectDuration);
    }

    public IEnumerator CloseInventory(Inventory inventoryToClose)
    {
        StartCoroutine(inventoryToClose.CloseInventoryCoroutine(closeEffectDuration, _leftHiddenPosition));

        yield return new WaitForSeconds(closeEffectDuration);
    }


    public async void OpenInventories()
    {
        OnInventoryOpen?.Invoke();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.75f, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position + Vector3.up), CurveType.EaseOutSin);
        _backFadeImage.UFadeImage(openEffectDuration, 0.5f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(heroesInventories[i].OpenInventoryCoroutine(openEffectDuration));
        }

        await Task.Delay((int)(openEffectDuration * 0.75f * 1000));

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.25f, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position), CurveType.EaseInSin);
    }

    public async void CloseInventories()
    {
        OnInventoryClose?.Invoke();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(closeEffectDuration, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_hiddenPosition.position), CurveType.EaseOutCubic);
        _backFadeImage.UFadeImage(closeEffectDuration, 0f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(heroesInventories[i].CloseInventoryCoroutine(closeEffectDuration));
        }
    }

    #endregion
}
