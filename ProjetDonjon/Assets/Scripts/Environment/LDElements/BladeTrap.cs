using System.Collections;
using UnityEngine;
using Utilities;

public class BladeTrap : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float fallDuration;
    [SerializeField] private float stayUpDuration;
    [SerializeField] private float stayDownDuration;
    [SerializeField] private float possibleOffsetAmplitude;
    [SerializeField] private float forcedOffset;


    [Header("References")]
    [SerializeField] private SpriteRenderer _trapSprite;
    [SerializeField] private Transform _endPosTr;
    [SerializeField] private BladeTrapCollider _bladeCollider;


    private void Start()
    {
        StartCoroutine(TrapBehaviorCoroutine());
    }


    private IEnumerator TrapBehaviorCoroutine()
    {
        if(forcedOffset != 0)
        {
            yield return new WaitForSeconds(forcedOffset);
        }
        else
        {
            yield return new WaitForSeconds(Random.Range(0, possibleOffsetAmplitude));
        }
        
        while(true)
        {
            yield return new WaitForSeconds(stayUpDuration);

            _trapSprite.transform.UChangeLocalPosition(fallDuration, _endPosTr.localPosition, CurveType.EaseInCubic);

            yield return new WaitForSeconds(fallDuration);

            _bladeCollider.canCollide = true;

            yield return new WaitForSeconds(stayDownDuration * 0.75f);

            _trapSprite.transform.UChangeLocalPosition(stayDownDuration * 0.25f, new Vector3(0, 0, 0), CurveType.EaseInOutCubic);
            _bladeCollider.canCollide = false;

            yield return new WaitForSeconds(stayDownDuration * 0.25f);
        }
    }
}
