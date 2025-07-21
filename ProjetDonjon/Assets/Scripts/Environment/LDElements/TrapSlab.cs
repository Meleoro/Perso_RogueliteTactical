using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

public class TrapSlab : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private ParticleSystem dustVFXPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private int arrowDamages;
    [SerializeField] private float cooldownDuration;
    [SerializeField] private float activatePosOffset;
    
    [Header("Private Infos")]
    private bool isOnCooldown;
    private bool isDeactivated;

    [Header("References")]
    [SerializeField] Transform[] _triggeredArrowDispensers;
    [SerializeField] Light2D _highlightLight;
    private ParticleSystem[] _dustParticleSystems;



    private void Start()
    {
        _dustParticleSystems = new ParticleSystem[_triggeredArrowDispensers.Length];

        for (int i = 0; i < _triggeredArrowDispensers.Length; i++)
        {
            _dustParticleSystems[i] = 
                Instantiate(dustVFXPrefab, _triggeredArrowDispensers[i].transform.position, _triggeredArrowDispensers[i].transform.rotation, transform);
        }
    }


    #region Chackboard Room

    public void Highlight()
    {
        _highlightLight.ULerpIntensity(0.2f, 0.5f);
    }

    public void StopHighlight()
    {
        _highlightLight.ULerpIntensity(0.2f, 0);
    }

    public void Deactivate()
    {
        isDeactivated = true;
    }

    #endregion


    private IEnumerator TriggerTrapCoroutine()
    {
        isOnCooldown = true;
        transform.UChangePosition(0.2f, transform.position + Vector3.down * activatePosOffset, CurveType.EaseOutCubic);

        for(int i = 0; i < _triggeredArrowDispensers.Length; i++)
        {
            if (isDeactivated) break;

            Arrow newArrow = Instantiate(arrowPrefab, _triggeredArrowDispensers[i].position, _triggeredArrowDispensers[i].rotation);
            newArrow.InitialiseArrow(arrowSpeed, arrowDamages);

            _dustParticleSystems[i].Play();
        }

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
        transform.UChangePosition(0.2f, transform.position - Vector3.down * activatePosOffset, CurveType.EaseOutCubic);
    }



    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isOnCooldown) return;
        if (!collision.CompareTag("Hero")) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Jump) return;

        StartCoroutine(TriggerTrapCoroutine());
    }
}
