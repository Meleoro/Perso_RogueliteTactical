using DG.Tweening;
using System.Collections;
using UnityEngine;

public class StunnedVFX : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private float amplitude;

    [Header("Private Infos")]
    private float timer1, timer2;
    private Coroutine effectCoroutine;

    [Header("References")]
    [SerializeField] private Transform star1;
    [SerializeField] private Transform star2;

    private void Start()
    {
        timer2 = 0.5f;

        star1.localScale = Vector3.zero;
        star2.localScale = Vector3.zero;
    }

    public void Appear()
    {
        star1.DOScale(Vector3.one * 0.7f, 0.2f).SetEase(Ease.OutElastic);
        star2.DOScale(Vector3.one * 0.7f, 0.2f).SetEase(Ease.OutElastic);

        effectCoroutine = StartCoroutine(EffectCoroutine());
    }

    private IEnumerator EffectCoroutine()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            timer1 += Time.deltaTime * speed;
            timer2 += Time.deltaTime * speed;
            timer1 = timer1 % 1;
            timer2 = timer2 % 1;

            float angle1 = Mathf.Lerp(0, 360, timer1) * Mathf.Deg2Rad;
            float angle2 = Mathf.Lerp(0, 360, timer2) * Mathf.Deg2Rad;

            Vector2 pos1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1) * 0.6f) * amplitude;
            Vector2 pos2 = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2) * 0.6f) * amplitude;

            star1.localPosition = pos1;
            star2.localPosition = pos2;
        }
    }

    public void Hide()
    {
        star1.DOScale(0f, 0.2f).SetEase(Ease.InElastic);
        star2.DOScale(0f, 0.2f).SetEase(Ease.OutElastic);

        StopCoroutine(effectCoroutine);
    }
}
