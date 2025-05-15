using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class UnitUI : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float appearEffectSpeed;

    [Header("Private Infos")]
    private float currentAimedRatio;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Image _healthFillableImage;
    [SerializeField] private Image _healthFillableImage2;
    [SerializeField] private Image _hearthImage;
    [SerializeField] private RectTransform _parentTr;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    [SerializeField] private Animator _animator;


    private void Start()
    {
        HideUnitUI();
    }

    private void Update()
    {
        _healthFillableImage.fillAmount = Mathf.Lerp(_healthFillableImage.fillAmount, currentAimedRatio, Time.deltaTime * 5f);
        _healthFillableImage2.fillAmount = Mathf.Lerp(_healthFillableImage2.fillAmount, _healthFillableImage.fillAmount, Time.deltaTime * 5f);
    }

    public void HideUnitUI()
    {
        _animator.SetBool("IsShowned", false);
    }

    public void ShowUnitUI()
    {
        _animator.SetBool("IsShowned", true);
    }

    public void ActualiseUI(float currentHealthRatio, int currentHealth)
    {
        currentAimedRatio = currentHealthRatio;
        _healthText.text = currentHealth.ToString();

        _hearthImage.rectTransform.UBounceScale(0.15f, Vector3.one * 1.3f, 0.3f, Vector3.one, CurveType.EaseInOutCubic);
        _hearthImage.rectTransform.UShakeLocalPosition(0.3f, 10f, 0.03f, ShakeLockType.Z);
        _parentTr.UShakeLocalPosition(0.3f, 20f, 0.03f, ShakeLockType.Z);
    }
}
