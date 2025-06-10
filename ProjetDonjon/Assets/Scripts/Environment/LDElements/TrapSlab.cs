using System.Collections;
using UnityEngine;
using Utilities;

public class TrapSlab : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private int arrowDamages;
    [SerializeField] private float cooldownDuration;
    [SerializeField] private float activatePosOffset;
    
    [Header("Private Infos")]
    private bool isOnCooldown;

    [Header("References")]
    [SerializeField] Transform[] _triggeredArrowDispensers;



    private IEnumerator TriggerTrapCoroutine()
    {
        isOnCooldown = true;
        transform.UChangePosition(0.2f, transform.position + Vector3.down * activatePosOffset, CurveType.EaseOutCubic);

        for(int i = 0; i < _triggeredArrowDispensers.Length; i++)
        {
            Arrow newArrow = Instantiate(arrowPrefab, _triggeredArrowDispensers[i].position, _triggeredArrowDispensers[i].rotation);
            newArrow.InitialiseArrow(arrowSpeed, arrowDamages);
        }

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
        transform.UChangePosition(0.2f, transform.position - Vector3.down * activatePosOffset, CurveType.EaseOutCubic);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOnCooldown) return;
        if (!collision.CompareTag("Hero")) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Jump) return;

        StartCoroutine(TriggerTrapCoroutine());
    }
}
