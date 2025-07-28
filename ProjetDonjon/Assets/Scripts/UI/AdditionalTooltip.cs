using TMPro;
using UnityEngine;

public class AdditionalTooltip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;


    private void Start()
    {
        Hide();
    }

    public void Show(AdditionalTooltipData data) 
    {
        _mainRectTr.localScale = Vector3.one;

        _nameText.text = data.tooltipName;
        _descriptionText.text = data.tooltipDescription;
    }

    public void Hide()
    {
        _mainRectTr.localScale = Vector3.zero;
    }
}
