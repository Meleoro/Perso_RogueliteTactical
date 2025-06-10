using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    struct SpawnerEnemyStruct
    {
        public AIUnit enemy;
        [Range(0, 100)] public int probability;
    }

    [Header("Parameters")]
    [SerializeField] private AIUnit[] possibleEnemies;

    [Header("Debug Parameters")]
    [SerializeField] private bool isDebugSpawer;
    [SerializeField] private Unit spawnedUnit;

    [Header("Public Infos")]
    [HideInInspector] public BattleTile associatedTile;

    
    public Unit GetSpawnedEnemy(int dangerAmountToFill)
    {
        if (isDebugSpawer)
        {
            return spawnedUnit;
        }

        int antiCrashCounter = 0;

        while(antiCrashCounter++ < 20)
        {
            int pickedIndex = Random.Range(0, possibleEnemies.Length);

            if(possibleEnemies[pickedIndex].AIData.dangerLevel <  dangerAmountToFill) 
                return possibleEnemies[pickedIndex];
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if(isDebugSpawer) Gizmos.color = Color.blue;
        else Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
