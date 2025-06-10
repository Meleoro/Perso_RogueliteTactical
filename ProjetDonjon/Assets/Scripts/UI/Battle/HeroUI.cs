using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class UnitUI : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float appearEffectSpeed;

    [Header("Actions")]
    public Action HoverAction;
    public Action UnHoverAction;
    public Action ClickAction;

    [Header("Private Infos")]
    private float currentAimedRatio;
    private int currentHealth;
    private Alteration[] currentAlterations;
    private Vector3 saveDamageTextPos;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Image _healthFillableImage;
    [SerializeField] private Image _healthFillableImage2;
    [SerializeField] private Image _hearthImage;
    [SerializeField] private RectTransform _parentTr;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    [SerializeField] private Animator _animator;
    [SerializeField] private RectTransform _alterationsParent;
    [SerializeField] private Alteration[] _alterations;
    [SerializeField] private TextMeshProUGUI _damageText;



    private void Start()
    {
        HideUnitUI();

        saveDamageTextPos = _damageText.rectTransform.localPosition;
        _damageText.rectTransform.localScale = Vector3.zero;
    }

    private void Update()
    {
        _healthFillableImage.fillAmount = Mathf.Lerp(_healthFillableImage.fillAmount, currentAimedRatio, Time.deltaTime * 5f);
        _healthFillableImage2.fillAmount = Mathf.Lerp(_healthFillableImage2.fillAmount, _healthFillableImage.fillAmount, Time.deltaTime * 5f);
    }


    #region Hide / Show

    public void HideUnitUI()
    {
        _animator.SetBool("IsShowned", false);
    }

    public async void ShowUnitUI()
    {
        await Task.Yield();

        _animator.SetBool("IsShowned", true);
    }

    #endregion


    #region Mouse Inputs

    public void HoverUnit()
    {
        HoverAction?.Invoke();
    }

    public void UnHoverUnit()
    {
        UnHoverAction?.Invoke();
    }

    public void ClickUnit() 
    {
        ClickAction?.Invoke();
    }

    #endregion


    #region Others

    public void ActualiseUI(float currentHealthRatio, int currentHealth, List<StatModificatorStruct> currentStatModificators)
    {
        if(currentAimedRatio != currentHealthRatio && currentAimedRatio != 0)
        {
            StartCoroutine(DoDamageTextEffectCoroutine(this.currentHealth - currentHealth, 0.8f));

            _hearthImage.rectTransform.UBounceScale(0.15f, Vector3.one * 1.3f, 0.3f, Vector3.one, CurveType.EaseInOutCubic);
            _hearthImage.rectTransform.UShakeLocalPosition(0.3f, 10f, 0.03f, ShakeLockType.Z);
            _parentTr.UShakeLocalPosition(0.3f, 20f, 0.03f, ShakeLockType.Z);
        }

        currentAimedRatio = currentHealthRatio;
        _healthText.text = currentHealth.ToString();
        this.currentHealth = currentHealth;

        for (int i = 0; i < _alterations.Length; i++)
        {
            if(i >= currentStatModificators.Count)
            {
                _alterations[i].Disappear();
                continue;
            }
            _alterations[i].Appear(currentStatModificators[i]);
        }
    }

    private IEnumerator DoDamageTextEffectCoroutine(int damages, float duration)
    {
        _damageText.text = "-" + damages;
        _damageText.rectTransform.localPosition = saveDamageTextPos;

        _damageText.rectTransform.UChangeScale(duration * 0.1f, new Vector3(1.3f, 0.9f, 1.0f), CurveType.EaseInOutCubic);
        _damageText.rectTransform.UChangeLocalPosition(duration, saveDamageTextPos + Vector3.up * 25f, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration * 0.1f);

        _damageText.rectTransform.UChangeScale(duration * 0.2f, new Vector3(0.85f, 1.15f, 1.0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _damageText.rectTransform.UChangeScale(duration * 0.1f, new Vector3(1f, 1f, 1.0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        _damageText.rectTransform.UChangeScale(duration * 0.2f, new Vector3(0f, 0f, 0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _damageText.rectTransform.localPosition = saveDamageTextPos;

    }

    #endregion
}
