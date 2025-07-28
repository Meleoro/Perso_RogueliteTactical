using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class DetailsPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float openingDuration;
    [SerializeField] private Vector3 offset;

    [Header("Private Infos")]
    [SerializeField] private Coroutine currentCoroutine;

    [Header("References")]
    [SerializeField] private RectTransform[] _statsParents;
    [SerializeField] private TextMeshProUGUI[] _statsTexts;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private RectTransform _parentRectTr;
    [SerializeField] private AdditionalTooltip[] _additionalTooltips;


    private void Start()
    {
        CloseDetails();
    }


    public void OpenDetails(LootData data, Vector3 lootPos)
    {
        _parentRectTr.position = lootPos + offset;
        _parentRectTr.gameObject.SetActive(true);

        if (data.healthUpgrade != 0)
        {
            _statsParents[0].gameObject.SetActive(true);
            _statsTexts[0].text = data.healthUpgrade.ToString();
        }
        if (data.strengthUpgrade != 0)
        {
            _statsParents[1].gameObject.SetActive(true);
            _statsTexts[1].text = data.strengthUpgrade.ToString();
        }
        if (data.speedUpgrade != 0)
        {
            _statsParents[2].gameObject.SetActive(true);
            _statsTexts[2].text = data.speedUpgrade.ToString();
        }
        if (data.luckUpgrade != 0)
        {
            _statsParents[3].gameObject.SetActive(true);
            _statsTexts[3].text = data.luckUpgrade.ToString();
        }

        _nameText.text = data.lootName;
        _descriptionText.text = data.lootDescription;

        for(int i = 0; i < _additionalTooltips.Length; i++)
        {
            if (i < data.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(data.additionalTooltipDatas[i]);

            else
                _additionalTooltips[i].Hide();
        }

        if(currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(OpenEffectsCoroutine(0.15f));
    }

    private IEnumerator OpenEffectsCoroutine(float duration)
    {
        float aimedSize = 0.75f;
        _parentRectTr.UStopChangeScale();
        _parentRectTr.UChangeScale(duration * 0.25f, new Vector3(1.25f * aimedSize, 0.85f * aimedSize, 1f * aimedSize), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.25f);

        _parentRectTr.UStopChangeScale();
        _parentRectTr.UChangeScale(duration * 0.5f, new Vector3(0.85f * aimedSize, 1.15f * aimedSize, 1f * aimedSize), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        _parentRectTr.UStopChangeScale();
        _parentRectTr.UChangeScale(duration * 0.25f, new Vector3(1f * aimedSize, 1f * aimedSize, 1f * aimedSize), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.25f);
    }

    public void CloseDetails()
    {
        for(int i = 0; i < _statsParents.Length; i++)
        {
            _statsParents[i].gameObject.SetActive(false);
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        _parentRectTr.localScale = new Vector3(0, 0.75f, 0);
        _parentRectTr.gameObject.SetActive(false);
    }

    private IEnumerator CloseEffectsCoroutine()
    {
        yield return null;
    }
}
