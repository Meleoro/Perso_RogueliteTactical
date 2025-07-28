using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

public class HeroInfosScreen : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float openDuration;
    [SerializeField] private float closeDuration;

    [Header("Actions")]
    public Action Open;
    public Action Close;

    [Header("Private Infos")]
    private HeroData heroData;
    private Hero hero;
    private Inventory currentInventory;
    private bool isOpenning;
    private int currentHeroIndex;

    [Header("References")]
    [SerializeField] private HeroesManager _heroesManager;
    [SerializeField] private TextMeshProUGUI[] _statsTexts;
    [SerializeField] private TextMeshProUGUI _heroName;
    [SerializeField] private RectTransform _shownInfoScreenPosition;
    [SerializeField] private RectTransform _hiddenInfoScreenPosition;
    [SerializeField] private RectTransform _shownInventoryPosition;
    [SerializeField] private RectTransform _hiddenInventoryPosition;
    [SerializeField] private RectTransform _mainRectParent;
    [SerializeField] private EquipmentSlot[] _equipmentSlots;

    [Header("References Buttons")]
    [SerializeField] private RectTransform _leftButton;
    [SerializeField] private RectTransform _rightButton;
    [SerializeField] private RectTransform _shownLeftButtonPosition;
    [SerializeField] private RectTransform _shownRightButtonPosition;


    private void Start()
    {
        for(int i = 0; i < _equipmentSlots.Length; i++)
        {
            _equipmentSlots[i].OnEquipmentAdd += AddEquipment;
            _equipmentSlots[i].OnEquipmentRemove += RemoveEquipment;
        }

        UIManager.Instance.OnStartDrag += StartDrag;
        UIManager.Instance.OnStopDrag += StopDrag;
    }


    public void ChangeHero(bool left)
    {
        if(left) currentHeroIndex = (--currentHeroIndex) % _heroesManager.Heroes.Length;
        else currentHeroIndex = (++currentHeroIndex) % _heroesManager.Heroes.Length;

        if (currentHeroIndex < 0) currentHeroIndex += _heroesManager.Heroes.Length;

        ActualiseInfoScreen(_heroesManager.Heroes[currentHeroIndex]);
        ActualiseInventory();
    }


    #region Open / Close Functions

    public IEnumerator OpenInfosScreenCoroutine()
    {
        ActualiseInfoScreen(_heroesManager.Heroes[_heroesManager.CurrentHeroIndex]);
        currentHeroIndex = _heroesManager.CurrentHeroIndex;

        HeroesManager.Instance.Heroes[currentHeroIndex].Controller.StopControl();

        Open.Invoke();

        isOpenning = true;
        currentInventory = hero.Inventory;

        currentInventory.RectTransform.position = _hiddenInventoryPosition.position;
        currentInventory.LootParent.position = _hiddenInventoryPosition.position;
        _mainRectParent.UChangePosition(openDuration, _shownInfoScreenPosition.position, CurveType.EaseOutCubic);
        currentInventory.RectTransform.UChangePosition(openDuration, _shownInventoryPosition.position, CurveType.EaseOutCubic);
        currentInventory.LootParent.UChangePosition(openDuration, _shownInventoryPosition.position, CurveType.EaseOutCubic);

        _leftButton.UChangePosition(openDuration, _shownLeftButtonPosition.position, CurveType.EaseOutCubic);
        _rightButton.UChangePosition(openDuration, _shownRightButtonPosition.position, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(openDuration);

        isOpenning = false;
    }

    public IEnumerator CloseInfosScreenCoroutine()
    {
        isOpenning = true;

        HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.RestartControl();

        Close.Invoke();

        _mainRectParent.UChangePosition(closeDuration, _hiddenInfoScreenPosition.position, CurveType.EaseOutCubic);
        currentInventory.RectTransform.UChangePosition(closeDuration, _hiddenInventoryPosition.position, CurveType.EaseOutCubic);
        currentInventory.LootParent.UChangePosition(closeDuration, _hiddenInventoryPosition.position, CurveType.EaseOutCubic);

        _leftButton.UChangePosition(openDuration, _shownLeftButtonPosition.position + new Vector3(-3f, 0, 0), CurveType.EaseOutCubic);
        _rightButton.UChangePosition(openDuration, _shownRightButtonPosition.position + new Vector3(3f, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(closeDuration);

        currentInventory.RebootPosition();

        isOpenning = false;
    }

    public bool VerifyCanOpenOrCloseHeroInfos()
    {
        return !isOpenning;
    }

    #endregion


    #region Manage Equipment

    private void AddEquipment(Loot equipment, int slotIndex)
    {
        hero.AddEquipment(equipment, slotIndex);

        ActualiseInfoScreen(hero);
    }

    private void RemoveEquipment(Loot equipment, int slotIndex)
    {
        hero.RemoveEquipment(equipment, slotIndex);

        ActualiseInfoScreen(hero);
    }

    #endregion


    #region Drag Functions

    private void StartDrag()
    {
        Loot draggedLoot = UIManager.Instance.DraggedLoot;

        if (draggedLoot.LootData.lootType != LootType.Equipment) return;

        for(int i = 0; i < _equipmentSlots.Length; i++)
        {
            if (draggedLoot.LootData.equipmentType == _equipmentSlots[i].EquipmentType)
            {
                _equipmentSlots[i].HightlightSlot();
            }
            else
            {
                _equipmentSlots[i].HideSlot();
            }
        }
    }

    private void StopDrag()
    {
        for (int i = 0; i < _equipmentSlots.Length; i++)
        {
            _equipmentSlots[i].GetBackToNormal();
        }
    }

    #endregion


    public EquipmentSlot GetAppropriateEquipmentSlot(EquipmentType type)
    {
        EquipmentSlot result = null;

        for (int i = 0; i < _equipmentSlots.Length; i++)
        {
            if (_equipmentSlots[i].EquipmentType == type && _equipmentSlots[i].EquipedLoot is null)
            {
                return _equipmentSlots[i];
            }
            else if(_equipmentSlots[i].EquipmentType == type)
            {
                result = _equipmentSlots[i];
            }
        }

        return result;
    }


    public void ActualiseInfoScreen(Hero hero)
    {
        this.heroData = hero.HeroData;
        this.hero = hero;   

        int currentHealth = heroData.baseHealth;
        int currentStrength = heroData.baseStrength;
        int currentSpeed = heroData.baseSpeed;
        int currentLuck = heroData.baseLuck;
        int currentMovePoints = heroData.baseMovePoints;

        for(int i = 0; i < hero.EquippedLoot.Length; i++)
        {
            _equipmentSlots[i].RemoveEquipment(false);

            if (hero.EquippedLoot[i] is null) continue;

            currentHealth += hero.EquippedLoot[i].LootData.healthUpgrade;
            currentStrength += hero.EquippedLoot[i].LootData.strengthUpgrade;
            currentSpeed += hero.EquippedLoot[i].LootData.speedUpgrade;
            currentLuck += hero.EquippedLoot[i].LootData.luckUpgrade;

            _equipmentSlots[i].AddEquipment(hero.EquippedLoot[i], false);
        }

        _statsTexts[0].text = currentHealth.ToString();
        _statsTexts[1].text = currentStrength.ToString();
        _statsTexts[2].text = currentSpeed.ToString();
        _statsTexts[3].text = currentLuck.ToString();

        hero.ActualiseUnitInfos(currentHealth, currentStrength, currentSpeed, currentLuck, currentMovePoints);
    }

    private void ActualiseInventory()
    {
        currentInventory.RebootPosition();

        currentInventory = hero.Inventory;

        currentInventory.RectTransform.position = _shownInventoryPosition.position;
        currentInventory.LootParent.position = _shownInventoryPosition.position;
    }
}
