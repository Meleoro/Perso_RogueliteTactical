using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
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
    [SerializeField] private Image _secondaryImage;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Animator _animator;

    
    public void Setup(RelicData relicData, bool isPossessed)
    {
        if (relicData == null)
        {
            _relicImage.gameObject.SetActive(false);
            return;
        }

        _relicImage.sprite = relicData.icon;
        _secondaryImage.sprite = relicData.icon;

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

    public IEnumerator NewRelicEffectCoroutine()
    {
        _animator.SetTrigger("GetNewRelic");

        //_rectTr.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.1f);
        Setup(relicData, true);

        //_rectTr.DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.InOutCubic);
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
