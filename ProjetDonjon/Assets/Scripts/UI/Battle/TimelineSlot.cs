using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class TimelineSlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color heroColor;
    [SerializeField] private Color enemyColor;
    [SerializeField] private Color summonColor;
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
        saveColorUnit = _unitImage.color;

        if (currentIndex == 0) _rectTr.localScale = Vector3.one * firstIndexSize;
        else _rectTr.localScale = Vector3.one * baseSize;

        unit = unitReference;

        if(unit.GetType() == typeof(Hero)) 
        {
            _mainImage.color = heroColor;
            saveColorMain = _mainImage.color;
        }
        else
        {
            _mainImage.color = enemyColor;
            saveColorMain = _mainImage.color;
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

    #region Mouse Events

    public void OverlaySlot()
    {
        unit.DisplayOverlayOutline();

        _mainImage.ULerpImageColor(0.2f, new Color(saveColorMain.r + 0.1f, saveColorMain.g + 0.1f, saveColorMain.b + 0.1f));
        _unitImage.ULerpImageColor(0.2f, new Color(saveColorUnit.r + 0.1f, saveColorUnit.g + 0.1f, saveColorUnit.b + 0.1f));
    }

    public void QuitOverlaySlot()
    {
        unit.HideOutline();

        _mainImage.ULerpImageColor(0.2f, new Color(saveColorMain.r, saveColorMain.g, saveColorMain.b));
        _unitImage.ULerpImageColor(0.2f, new Color(saveColorUnit.r, saveColorUnit.g, saveColorUnit.b));
    }

    public void ClickSlot()
    {
        CameraManager.Instance.FocusOnPosition(unit.transform.position, 3.5f);
    }

    #endregion
}
