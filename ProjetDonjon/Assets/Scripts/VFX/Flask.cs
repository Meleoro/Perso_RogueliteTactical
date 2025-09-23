using System.Collections;
using UnityEngine;

public enum FlaskType
{
    Poison,
    Debuff,
    Cure
}

public class Flask : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private GameObject explosionPoisonPrefab;
    [SerializeField] private GameObject explosionDebuffPrefab;
    [SerializeField] private GameObject explosionCurePrefab;
    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve yCurve;
    [SerializeField] private AnimationCurve scaleCurve;


    public void Initialise(Vector3 aimedPosition, FlaskType flaskType)
    {
        StartCoroutine(FlaskCoroutine(aimedPosition, flaskType));
    }

    private IEnumerator FlaskCoroutine(Vector3 aimedPosition, FlaskType flaskType)
    {
        float timer = 0;
        Vector3 startPos = transform.position;   

        while(timer < duration)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(startPos, aimedPosition, timer / duration);
            transform.position += new Vector3(0, yCurve.Evaluate(timer / duration));

            transform.localScale = Vector3.one * scaleCurve.Evaluate(timer / duration);

            transform.rotation *= Quaternion.Euler(0, 0, Time.deltaTime * 10);

            yield return new WaitForEndOfFrame();
        }

        ReachDestination(flaskType);
    }

    private void ReachDestination(FlaskType flaskType)
    {
        switch (flaskType)
        {
            case FlaskType.Poison:
                Destroy(Instantiate(explosionPoisonPrefab, transform.position, Quaternion.Euler(0, 0, 0)), 2f);
                break;

            case FlaskType.Debuff:
                Destroy(Instantiate(explosionDebuffPrefab, transform.position, Quaternion.Euler(0, 0, 0)), 2f);
                break;

            case FlaskType.Cure:
                Destroy(Instantiate(explosionCurePrefab, transform.position, Quaternion.Euler(0, 0, 0)), 2f);
                break;
        }

        Destroy(gameObject);
    }
}
