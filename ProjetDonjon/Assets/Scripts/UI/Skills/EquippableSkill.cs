using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class EquippableSkill : MonoBehaviour
{
    [Header("Actions")]
    public Action<EquippableSkill> OnStartDrag;
    public Action OnEndDrag;

    [Header("Private Infos")]
    private SkillData skillData;
    private PassiveData passiveData;
    private Coroutine dragCoroutine;

    [Header("Public Infos")]
    public SkillData SkillData { get { return skillData; } }
    public PassiveData PassiveData { get { return passiveData; } }

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _mainImage;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image[] _skillPointsIcons;


    private void ActualiseDetails()
    {
        if(skillData is not null)
        {
            _buttonText.text = skillData.skillName;
            _skillIcon.sprite = skillData.skillIcon;

            for (int i = 0; i < _skillPointsIcons.Length; i++)
            {
                if(skillData.skillPointCost <= i)
                {
                    _skillPointsIcons[i].gameObject.SetActive(false);
                    continue;
                }

                _skillPointsIcons[i].gameObject.SetActive(true);
            }
        }
        else if(passiveData is not null) 
        {
            _buttonText.text = passiveData.passiveName;
            _skillIcon.sprite = passiveData.passiveIcon;

            for(int i = 0; i < _skillPointsIcons.Length; i++)
            {
                _skillPointsIcons[i].gameObject.SetActive(false);
            }
        }
    }


    #region Inputs

    public void HoverButton()
    {
        _mainRectTr.UChangeScale(0.2f, Vector3.one * 1.05f, CurveType.EaseOutSin);
    }

    public void UnhoverButton()
    {
        _mainRectTr.UChangeScale(0.2f, Vector3.one * 0.95f, CurveType.EaseOutSin);
    }

    public void StartDrag()
    {
        OnStartDrag?.Invoke(this);
    }

    public void EndDrag()
    {
        OnEndDrag?.Invoke();
    }

    #endregion


    #region Hide / Show

    public void Show(SkillData data)
    {
        _mainRectTr.gameObject.SetActive(true);

        passiveData = null;
        skillData = data;

        ActualiseDetails();
    }

    public void Show(PassiveData data)
    {
        _mainRectTr.gameObject.SetActive(true);

        skillData = null;
        passiveData = data;

        ActualiseDetails();
    }

    public void Hide()
    {
        _mainRectTr.gameObject.SetActive(false);
    }

    #endregion


    #region Drag Preview 

    public void ShowDragPreview(SkillData data)
    {
        gameObject.SetActive(true);

        passiveData = null;
        skillData = data;
        ActualiseDetails();

        Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        _mainRectTr.position = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);

        dragCoroutine = StartCoroutine(DragCoroutine());
    }

    public void ShowDragPreview(PassiveData data)
    {
        gameObject.SetActive(true);

        passiveData = data;
        skillData = null;
        ActualiseDetails();

        Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        _mainRectTr.position = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);

        dragCoroutine = StartCoroutine(DragCoroutine());
    }

    private IEnumerator DragCoroutine()
    {
        _mainImage.raycastTarget = false;
        _mainImage.color = new Color(1, 1, 1, 0.5f);
        _skillIcon.color = new Color(1, 1, 1, 0.5f);
        _buttonText.color = new Color(1, 1, 1, 0.5f);

        Vector3 dragWantedPos;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
            dragWantedPos = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);

            _mainRectTr.position = Vector3.Lerp(_mainRectTr.position, dragWantedPos, Time.deltaTime * 10f);
        }
    }

    public void HideDragPreview()
    {
        StopCoroutine(dragCoroutine);

        gameObject.SetActive(false);
    }

    #endregion 
}
