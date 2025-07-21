using UnityEngine;

public class VFXDestroyer : MonoBehaviour
{
    [SerializeField] private float timeBeforeDestroy;

    private void Start()
    {
        Destroy(gameObject, timeBeforeDestroy);
    }
}
