using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class InventoryActionPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 offset;

    [Header("Private Infos")]
    private Loot currentLoot;
    private bool isOpened;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform[] _buttonsRectTr;
    [SerializeField] private TextMeshProUGUI[] _buttonsTexts;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
    [SerializeField] private Animator _animator;


    public void OpenPanel(Loot associatedLoot)
    {
        if (isOpened && (associatedLoot == currentLoot))
        {
            ClosePanel();
            return;
        }

        currentLoot = associatedLoot;
        _animator.SetBool("IsOpened", true);
        isOpened = true;

        _buttonsRectTr[0].gameObject.SetActive(false);
        switch (associatedLoot.LootData.lootType)
        {
            case LootType.Equipment:
                _buttonsRectTr[0].gameObject.SetActive(true);
                if(associatedLoot.IsEquipped) _buttonsTexts[0].text = "UNEQUIP";
                else _buttonsTexts[0].text = "EQUIP";
                break;

            case LootType.Consumable:
                _buttonsRectTr[0].gameObject.SetActive(true);
                _buttonsTexts[0].text = "USE";
                break;
        }

        _mainRectTr.position = associatedLoot.Image.rectTransform.position + offset;
    }

    public void ClosePanel()
    {
        _animator.SetBool("IsOpened", false);
        isOpened = false;
    }


    public void HoverButton(int index)
    {
        _buttonsRectTr[index].UChangeScale(0.1f, Vector3.one * 1.1f);
        _buttonsRectTr[index].sizeDelta = new Vector2(_buttonsRectTr[index].sizeDelta.x, 36);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalLayoutGroup.transform as RectTransform);
    }

    public void UnhoverButton(int index)
    {
        _buttonsRectTr[index].UChangeScale(0.1f, Vector3.one);
        _buttonsRectTr[index].sizeDelta = new Vector2(_buttonsRectTr[index].sizeDelta.x, 30);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalLayoutGroup.transform as RectTransform);
    }


    public void PressUse()
    {
        switch (currentLoot.LootData.lootType)
        {
            case LootType.Equipment:
                if (currentLoot.IsEquipped) currentLoot.Unequip();
                else currentLoot.Equip(null);
                break;

            case LootType.Consumable:
                currentLoot.AssociatedHero.UseItem(currentLoot.LootData);
                currentLoot.DestroyItem();
                break;
        }

        ClosePanel();
    }

    public void PressThrow()
    {
        currentLoot.BecomeWorldItem();

        ClosePanel();
    }
}
