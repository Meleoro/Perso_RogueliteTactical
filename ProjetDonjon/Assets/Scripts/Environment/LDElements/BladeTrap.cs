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
    [SerializeField] private float ceilingMatFloatValue;


    [Header("References")]
    [SerializeField] private SpriteRenderer _trapSprite;
    [SerializeField] private Transform _endPosTr;
    [SerializeField] private BladeTrapCollider _bladeCollider;
    [SerializeField] private ParticleSystem _sparksVFX;
    [SerializeField] private SpriteLayerer _spriteLayerer;


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
            _trapSprite.material.ULerpMaterialFloat(fallDuration, 0, "_AddedY");

            yield return new WaitForSeconds(fallDuration);

            _sparksVFX.Play();
            _spriteLayerer.publicOffset = -50;
            _bladeCollider.canCollide = true;

            yield return new WaitForSeconds(stayDownDuration * 0.6f);

            _spriteLayerer.publicOffset = 0;
            _trapSprite.transform.UChangeLocalPosition(stayDownDuration * 0.4f, new Vector3(0, 0, 0), CurveType.EaseInOutCubic);
            _trapSprite.material.ULerpMaterialFloat(fallDuration * 0.4f, ceilingMatFloatValue, "_AddedY");
            _bladeCollider.canCollide = false;

            yield return new WaitForSeconds(stayDownDuration * 0.4f);
        }
    }
}
