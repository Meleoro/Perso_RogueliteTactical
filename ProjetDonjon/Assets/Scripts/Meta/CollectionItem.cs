using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class CollectionRelic : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color hiddenColor;

    [Header("Private Infos")]
    private RelicData relicData;
    private bool isPossessed;

    [Header("References")]
    [SerializeField] private Image _relicImage;
    [SerializeField] private RectTransform _rectTr;

    public void Setup(RelicData relicData, bool isPossessed)
    {
        if (relicData == null)
        {
            _relicImage.gameObject.SetActive(false);
            return;
        }

        _relicImage.sprite = relicData.icon;

        this.relicData = relicData;
        this.isPossessed = isPossessed;

        if (isPossessed)
        {
            _relicImage.color = Color.white;
        }
        else
        {
            _relicImage.color = hiddenColor;
        }
    }


    #region Mouse Functions

    public void Hover()
    {
        _rectTr.UChangeScale(0.2f, Vector3.one * 1.2f, CurveType.EaseOutSin);
    }

    public void Unhover()
    {
        _rectTr.UChangeScale(0.2f, Vector3.one, CurveType.EaseOutSin);
    }

    #endregion
}
