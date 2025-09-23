using UnityEngine;

public class PoisonedVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem _constantPoisonVFX;
    [SerializeField] private ParticleSystem _damagePoisonVFX;
    [SerializeField] private Animator _animator;

    public void PlayStartPoisonEffect() 
    {
        _constantPoisonVFX.Play();
        _damagePoisonVFX.Play();

        _animator.SetTrigger("Apply");
    }

    public void PlayApplyPoisonEffect()
    {
        _damagePoisonVFX.Play();

        _animator.SetTrigger("Apply");
    }

    public void StopPoisonEffect()
    {
        _constantPoisonVFX.Stop();
    }
}
