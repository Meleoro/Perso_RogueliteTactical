using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using Utilities;

public class AdditionalTooltip : MonoBehaviour
{
    [Header("Private Infos")]
    private Coroutine showCoroutine;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;


    private void Start()
    {
        Hide();
    }

    public void Show(AdditionalTooltipData data, float delay) 
    {
        _nameText.text = data.tooltipName;
        _descriptionText.text = data.tooltipDescription;

        if (showCoroutine is not null) StopCoroutine(showCoroutine);
        _mainRectTr.DOComplete();

        showCoroutine = StartCoroutine(ShowCoroutine(delay));
    }

    private IEnumerator ShowCoroutine(float delay)
    {
        _mainRectTr.localScale = Vector3.zero;

        yield return new WaitForSeconds(delay);

        _mainRectTr.DOScale(Vector3.one * 1.15f, 0.12f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.12f);

        _mainRectTr.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.InOutCubic);
    }


    public void Hide()
    {
        if (showCoroutine is not null) StopCoroutine(showCoroutine);
        _mainRectTr.DOComplete();

        _mainRectTr.localScale = Vector3.zero;
    }
}
