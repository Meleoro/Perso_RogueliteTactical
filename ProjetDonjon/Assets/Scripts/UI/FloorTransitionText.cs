using System.Collections;
using TMPro;
using UnityEngine;
using Utilities;

public class FloorTransitionText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _floorCounterText;


    public IEnumerator IntroCoroutine(float duration)
    {
        _mainText.UFadeText(duration * 0.2f, 1);
        _floorCounterText.UFadeText(duration * 0.2f, 1);

        yield return new WaitForSeconds(duration * 0.6f);

        UIManager.Instance.FadeScreen(duration * 0.3f, 0);

        yield return new WaitForSeconds(duration * 0.2f);

        _mainText.UFadeText(duration * 0.2f, 0);
        _floorCounterText.UFadeText(duration * 0.2f, 0);
    }


    public IEnumerator ChangeFloorCoroutine(int newFloor, float duration)
    {
        _mainText.UFadeText(duration * 0.2f, 1);
        _floorCounterText.UFadeText(duration * 0.2f, 1);

        yield return new WaitForSeconds(duration * 0.3f);

        Color saveColor = _floorCounterText.color;
        _floorCounterText.rectTransform.UBounceScale(duration * 0.05f, Vector3.one * 1.4f, duration * 0.15f, Vector3.one);
        _floorCounterText.UBounceTextColor(duration * 0.05f, Color.white, duration * 0.15f, saveColor);

        yield return new WaitForSeconds(duration * 0.04f);

        _floorCounterText.text = newFloor.ToString();

        yield return new WaitForSeconds(duration * 0.5f);

        _mainText.UFadeText(duration * 0.2f, 0);
        _floorCounterText.UFadeText(duration * 0.2f, 0);
    }
}
