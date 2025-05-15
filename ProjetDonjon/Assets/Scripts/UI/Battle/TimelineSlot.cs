using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class TimelineSlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color heroColor;
    [SerializeField] private Color enemyColor;
    [SerializeField] private float baseSize;
    [SerializeField] private float firstIndexSize;

    [Header("Private Infos")]
    private Unit unit;
    private int currentIndex;

    [Header("Public Infos")]
    public Unit Unit { get { return unit; } }

    [Header("References")]
    [SerializeField] private Image _unitImage;
    [SerializeField] private Image _mainImage;
    public RectTransform _rectTr;


    public void SetupSlot(Sprite unitSprite, Unit unitReference, int index)
    {
        currentIndex = index;

        _unitImage.sprite = unitSprite;
        _unitImage.SetNativeSize();

        if (currentIndex == 0) _rectTr.localScale = Vector3.one * firstIndexSize;
        else _rectTr.localScale = Vector3.one * baseSize;

        unit = unitReference;

        if(unit.GetType() == typeof(Hero)) 
        {
            _mainImage.color = heroColor;
        }
        else
        {
            _mainImage.color = enemyColor;
        }
    }

    public void Advance(float effectDuration, RectTransform[] slotPositions)
    {
        currentIndex--;
        _rectTr.UChangeLocalPosition(effectDuration, slotPositions[currentIndex].localPosition, CurveType.EaseOutCubic);

        if(currentIndex == 0)
        {
            _rectTr.UChangeScale(effectDuration, Vector3.one * firstIndexSize, CurveType.EaseOutSin);
        }
    }

    public void DestroySlot()
    {
        Destroy(gameObject);
    }
}
