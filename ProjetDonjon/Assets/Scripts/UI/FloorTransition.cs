using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorTransition : MonoBehaviour
{
    [Header("Private Infos")]
    private EnviroData currentEnviroData;
    private RectTransform[] buttonsRectTr;

    [Header("References")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _flootCounterText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelCounterText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _stopButton;


    private void Start()
    {
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 1);

        buttonsRectTr = new RectTransform[2];
        buttonsRectTr[0] = _continueButton.GetComponent<RectTransform>();
        buttonsRectTr[1] = _stopButton.GetComponent<RectTransform>();
    }

    public void StartTransition(EnviroData enviroData, int floorIndex)
    {
        currentEnviroData = enviroData;

        if (floorIndex == 0)
        {
            _recommandedLevelText.gameObject.SetActive(false);
            _recommandedLevelCounterText.gameObject.SetActive(false);

            _continueButton.gameObject.SetActive(false);
            _stopButton.gameObject.SetActive(false);

            StartCoroutine(IntroCoroutine(2f));
        }
        else
        {
            _recommandedLevelText.gameObject.SetActive(true);
            _recommandedLevelCounterText.gameObject.SetActive(true);

            _continueButton.gameObject.SetActive(true);
            _stopButton.gameObject.SetActive(true);

            _continueButton.enabled = true;
            _stopButton.enabled = true;

            StartCoroutine(ChangeFloorCoroutine(floorIndex, 2.5f));
        }
    }

    public void FadeScreen(float duration, float endValue)
    {
        _fadeImage.DOFade(endValue, duration);
    }



    #region Effects Coroutines

    private IEnumerator IntroCoroutine(float duration)
    {
        _floorText.DOFade(1, duration * 0.2f);
        _flootCounterText.DOFade(1, duration * 0.2f);

        yield return new WaitForSeconds(duration * 0.6f);

        FadeScreen(duration * 0.3f, 0);

        yield return new WaitForSeconds(duration * 0.2f);

        _floorText.DOFade(0, duration * 0.2f);
        _flootCounterText.DOFade(0, duration * 0.2f);
    }

    private IEnumerator ChangeFloorCoroutine(int newFloor, float duration)
    {
        _floorText.DOFade(1, duration * 0.2f);
        _flootCounterText.DOFade(1, duration * 0.2f);

        FadeScreen(1, 1);

        yield return new WaitForSeconds(duration * 0.3f);

        Color saveColor = _flootCounterText.color;
        _flootCounterText.rectTransform.DOScale(Vector3.one * 1.4f, duration * 0.05f);
        _flootCounterText.DOColor(Color.white, duration * 0.05f);

        yield return new WaitForSeconds(duration * 0.05f);

        _flootCounterText.rectTransform.DOScale(Vector3.one, duration * 0.15f);
        _flootCounterText.DOColor(saveColor, duration * 0.15f);

        _recommandedLevelText.DOFade(1, duration * 0.2f);
        _recommandedLevelCounterText.DOFade(1, duration * 0.2f);

        _continueButton.image.DOFade(1, duration * 0.2f);
        _stopButton.image.DOFade(1, duration * 0.2f);

        _recommandedLevelCounterText.text = currentEnviroData.recommandedLevels[newFloor - 1].ToString();
        _flootCounterText.text = newFloor.ToString();
    }

    private IEnumerator ContinueCoroutine(float duration)
    {
        _floorText.DOFade(0, duration);
        _flootCounterText.DOFade(0, duration);

        _recommandedLevelText.DOFade(0, duration);
        _recommandedLevelCounterText.DOFade(0, duration);

        _continueButton.image.DOFade(0, duration);
        _stopButton.image.DOFade(0, duration);

        FadeScreen(duration, 0);

        yield return new WaitForSeconds(duration);
    }

    private IEnumerator StopCoroutine(float duration)
    {
        _floorText.DOFade(0, duration);
        _flootCounterText.DOFade(0, duration);

        _recommandedLevelText.DOFade(0, duration);
        _recommandedLevelCounterText.DOFade(0, duration);

        _continueButton.image.DOFade(0, duration);
        _stopButton.image.DOFade(0, duration);

        yield return new WaitForSeconds(duration);

        StartCoroutine(GameManager.Instance.EndExplorationCoroutine());
    }

    #endregion


    #region Buttons 

    public void HoverButton(int buttonIndex)
    {
        buttonsRectTr[buttonIndex].DOKill();
        buttonsRectTr[buttonIndex].DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutCubic);
    }

    public void UnhoverButton(int buttonIndex)
    {
        buttonsRectTr[buttonIndex].DOKill();
        buttonsRectTr[buttonIndex].DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.InCubic);
    }

    public void ClickContinue()
    {
        _continueButton.enabled = false;
        _stopButton.enabled = false;

        StartCoroutine(ContinueCoroutine(1f));
    }

    public void ClickStop()
    {
        _continueButton.enabled = false;
        _stopButton.enabled = false;

        StartCoroutine(StopCoroutine(1f));
    }

    #endregion
}
