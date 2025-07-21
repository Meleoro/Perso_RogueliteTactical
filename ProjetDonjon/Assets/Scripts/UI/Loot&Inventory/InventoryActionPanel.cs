using System.Collections;
using UnityEngine;
using Utilities;

public class InventoryActionPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 offset;

    [Header("Private Infos")]
    private Loot currentLoot;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform[] _buttonsRectTr;
    [SerializeField] private Animator _animator;


    public void OpenPanel(Loot associatedLoot)
    {
        currentLoot = associatedLoot;
        _animator.SetBool("IsOpened", true);

        _mainRectTr.position = associatedLoot.Image.rectTransform.position + offset;
    }

    public void ClosePanel()
    {
        _animator.SetBool("IsOpened", false);
    }


    public void HoverButton(int index)
    {
        _buttonsRectTr[index].UChangeScale(0.2f, Vector3.one * 1.2f);
    }

    public void UnhoverButton(int index)
    {
        _buttonsRectTr[index].UChangeScale(0.2f, Vector3.one);
    }


    public void PressUse()
    {
        currentLoot.AssociatedHero.UseItem(currentLoot.LootData);

        currentLoot.DestroyItem();
        ClosePanel();
    }

    public void PressThrow()
    {

        ClosePanel();
    }
}
