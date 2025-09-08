using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ShieldVFX : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField][ColorUsage(true, true)] private Color maxGlowColor;
    [SerializeField][ColorUsage(true, true)] private Color normalGlowColor;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void PlayEquipAnim()
    {
        _animator.SetTrigger("Appear");

        StartCoroutine(FlashCoroutine(0.25f, false));
    }

    public void PlayBlockAnim()
    {
        _animator.SetTrigger("Block");

        StartCoroutine(FlashCoroutine(0.25f, false));
    }

    public void PlayBreakAnim()
    {
        _animator.SetTrigger("Break");

        StartCoroutine(FlashCoroutine(0.25f, true));
    }

    private IEnumerator FlashCoroutine(float duration, bool shake)
    {
        if (shake) transform.DOShakePosition(duration, 0.5f);

        _spriteRenderer.material.DOColor(maxGlowColor, "_GlowColor", duration * 0.33f);

        yield return new WaitForSeconds(duration * 0.33f);

        _spriteRenderer.material.DOColor(normalGlowColor, "_GlowColor", duration * 0.66f);
    }
}
