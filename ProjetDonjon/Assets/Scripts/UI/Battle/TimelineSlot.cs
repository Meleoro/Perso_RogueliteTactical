using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class TimelineSlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color currentTurnColor;
    [SerializeField] private Color baseColor;
    [SerializeField] private Color unitCurrentTurnColor;
    [SerializeField] private Color unitBaseColor;
    [SerializeField] private float baseSize;
    [SerializeField] private float firstIndexSize;

    [Header("Private Infos")]
    private Unit unit;
    private int currentIndex;
    private Color saveColorUnit;
    private Color saveColorMain;

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

        if(currentIndex == 0) 
        {
            _mainImage.color = currentTurnColor;
            saveColorMain = currentTurnColor;

            _unitImage.color = unitCurrentTurnColor;
            saveColorUnit = unitCurrentTurnColor;
        }
        else
        {
            _mainImage.color = baseColor;
            saveColorMain = baseColor;

            _unitImage.color = unitBaseColor;
            saveColorUnit = unitBaseColor;
        }
    }

    public void Advance(float effectDuration, RectTransform[] slotPositions)
    {
        currentIndex--;
        _rectTr.UChangeLocalPosition(effectDuration, slotPositions[currentIndex].localPosition, CurveType.EaseOutCubic);

        if(currentIndex == 0)
        {
            _rectTr.UChangeScale(effectDuration, Vector3.one * firstIndexSize, CurveType.EaseOutSin);

            _mainImage.color = baseColor;
            saveColorMain = baseColor;

            _unitImage.color = unitBaseColor;
            saveColorUnit = unitBaseColor;

            _unitImage.sprite = unit.UnitData.unitImageHighlight;
        }
    }

    public void DestroySlot()
    {
        Destroy(gameObject);
    }

    #region Mouse Events

    public void OverlaySlot()
    {
        unit.DisplayOverlayOutline();

        _unitImage.sprite = unit.UnitData.unitImageHighlight;

        _mainImage.ULerpImageColor(0.2f, new Color(saveColorMain.r + 0.1f, saveColorMain.g + 0.1f, saveColorMain.b + 0.1f));
        _unitImage.ULerpImageColor(0.2f, new Color(saveColorUnit.r + 0.1f, saveColorUnit.g + 0.1f, saveColorUnit.b + 0.1f));
    }

    public void QuitOverlaySlot()
    {
        unit.HideOutline();

        if(currentIndex != 0) 
            _unitImage.sprite = unit.UnitData.unitImage;

        _mainImage.ULerpImageColor(0.2f, new Color(saveColorMain.r, saveColorMain.g, saveColorMain.b));
        _unitImage.ULerpImageColor(0.2f, new Color(saveColorUnit.r, saveColorUnit.g, saveColorUnit.b));
    }

    public void ClickSlot()
    {
        CameraManager.Instance.FocusOnPosition(unit.transform.position, 3.5f);
    }

    #endregion
}
