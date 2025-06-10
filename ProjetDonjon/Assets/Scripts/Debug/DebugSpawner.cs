using UnityEngine;

public class DebugSpawner : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Unit unitToSpawn;

    public Unit GetSpawnedUnit()
    {
        return unitToSpawn;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
