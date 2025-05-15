using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class SkillsPanelButton : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color overlayActionAddedColor;
    [SerializeField] private Color clickedActionAddedColor;
    [SerializeField] private Color impossibleActionRemovedColor;

    [Header("Actions")]
    public Action<SkillsPanelButton> OnButtonClick;
    public Action<SkillData> OnSkillOverlay;
    public Action OnSkillQuitOverlay;

    [Header("Private Infos")]
    private SkillData skillData;
    private Color colorSave;
    private bool canBeUsed;
    private bool isClicked;

    [Header("Public Infos")]
    public SkillData SkillData { get { return skillData; } }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _buttonImage;


    private void Start()
    {
        colorSave = _buttonImage.color;
    }

    public void InitialiseButton(HeroSkillStruct heroSkillStruct, Hero hero)
    {
        skillData = heroSkillStruct.skill;
        _buttonText.text = skillData.skillName;

        canBeUsed = true;
    }


    #region Overlay / Click Functions

    public void OverlayButton()
    {
        if (isClicked)
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave + overlayActionAddedColor + clickedActionAddedColor);
            _buttonImage.rectTransform.UChangeScale(0.1f, Vector3.one * 1.1f);
        }
        else
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave + overlayActionAddedColor);
            _buttonImage.rectTransform.UChangeScale(0.1f, Vector3.one * 1f);

            OnSkillOverlay.Invoke(skillData);
        }

        BattleManager.Instance.DisplayPossibleSkillTiles(skillData, BattleManager.Instance.CurrentUnit.CurrentTile);
    }

    public void QuitOverlayButton(bool noActionCall = false)
    {
        if (isClicked)
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave + clickedActionAddedColor);
            _buttonImage.rectTransform.UChangeScale(0.1f, Vector3.one * 1.05f);
        }
        else
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave);
            _buttonImage.rectTransform.UChangeScale(0.1f, Vector3.one * 0.95f);

            if(!noActionCall)
                OnSkillQuitOverlay.Invoke();
        }
    }

    public void ClickButton()
    {
        if (!canBeUsed) return;

        StartCoroutine(ClickEffectCoroutine());

        if (isClicked) return;

        isClicked = true;
        OnButtonClick.Invoke(this);
    }

    public void Unclick()
    {
        isClicked = false;
        QuitOverlayButton(true);
    }


    private IEnumerator ClickEffectCoroutine()
    {
        _buttonImage.ULerpImageColor(0.1f, colorSave - clickedActionAddedColor);
        _buttonImage.rectTransform.UChangeScale(0.1f, Vector3.one * 0.85f);

        yield return new WaitForSeconds(0.1f);

        _buttonImage.ULerpImageColor(0.2f, colorSave + clickedActionAddedColor + overlayActionAddedColor);
        _buttonImage.rectTransform.UChangeScale(0.2f, Vector3.one * 1.05f);
    }

    #endregion
}
