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
    [SerializeField] private RectTransform _gloabalParentRectTr;
    [SerializeField] private RectTransform _mainParentRectTr;
    [SerializeField] private AdditionalTooltip[] _additionalTooltips;


    private void Start()
    {
        CloseDetails();
    }


    public void OpenDetails(LootData data, Vector3 lootPos)
    {
        _gloabalParentRectTr.position = lootPos + (offset + new Vector3(0.9f * data.spaceTaken[0].row.Length, 0));
        _gloabalParentRectTr.gameObject.SetActive(true);

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
                _additionalTooltips[i].Show(data.additionalTooltipDatas[i], 0.15f + i * 0.15f);

            else
                _additionalTooltips[i].Hide();
        }

        if(currentCoroutine is not null) StopCoroutine(currentCoroutine);
        
        currentCoroutine = StartCoroutine(OpenEffectsCoroutine(0.25f));
    }

    private IEnumerator OpenEffectsCoroutine(float duration)
    {
        _mainParentRectTr.localScale = Vector3.zero;

        float aimedSize = 0.75f;
        _mainParentRectTr.UChangeScale(0.12f, Vector3.one * aimedSize * 1.1f, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.12f);

        _mainParentRectTr.UChangeScale(0.15f, new Vector3(aimedSize, aimedSize, aimedSize), CurveType.EaseInOutCubic);
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

        _gloabalParentRectTr.gameObject.SetActive(false);

        if (currentCoroutine is not null) StopCoroutine(currentCoroutine);
        _mainParentRectTr.UStopChangeScale();

    }

    private IEnumerator CloseEffectsCoroutine()
    {
        yield return null;
    }
}
