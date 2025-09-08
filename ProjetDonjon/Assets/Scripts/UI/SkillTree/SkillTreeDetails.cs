using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class SkillTreeDetails : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float openingDuration;
    [SerializeField] private Vector3 offset;

    [Header("Private Infos")]
    [SerializeField] private Coroutine currentCoroutine;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _nodeTypeText;
    [SerializeField] private RectTransform _gloabalParentRectTr;
    [SerializeField] private RectTransform _mainParentRectTr;
    [SerializeField] private Image[] _skillPointsImages;
    [SerializeField] private AdditionalTooltip[] _additionalTooltips;


    private void Start()
    {
        CloseDetails();
    }


    public void OpenDetails(SkillTreeNodeData data, Vector3 lootPos, bool reverse)
    {

        Vector3 finalOffset = reverse ? offset : -offset;
        _gloabalParentRectTr.position = lootPos + finalOffset;
        _gloabalParentRectTr.gameObject.SetActive(true);

        // If it's a skill
        if(data.skillData is not null)
        {
            _nameText.text = data.skillData.skillName;
            _descriptionText.text = data.skillData.skillDescription;
            _nodeTypeText.text = "SKILL";

            for (int i = 0; i < _additionalTooltips.Length; i++)
            {
                if (i < data.skillData.additionalTooltipDatas.Length)
                    _additionalTooltips[i].Show(data.skillData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

                else
                    _additionalTooltips[i].Hide();
            }

            for(int i = 0; i < _skillPointsImages.Length; i++)
            {
                if (data.skillData.skillPointCost > i) _skillPointsImages[i].gameObject.SetActive(true);
                else _skillPointsImages[i].gameObject.SetActive(false);
            }
        }

        // if it's a passive
        else
        {
            _nameText.text = data.passiveData.passiveName;
            _descriptionText.text = data.passiveData.passiveDescription;
            _nodeTypeText.text = "PASSIVE";

            for (int i = 0; i < _additionalTooltips.Length; i++)
            {
                if (i < data.passiveData.additionalTooltipDatas.Length)
                    _additionalTooltips[i].Show(data.passiveData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

                else
                    _additionalTooltips[i].Hide();
            }

            for (int i = 0; i < _skillPointsImages.Length; i++)
            {
                _skillPointsImages[i].gameObject.SetActive(false);
            }
        }

        if (currentCoroutine is not null) StopCoroutine(currentCoroutine);

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
