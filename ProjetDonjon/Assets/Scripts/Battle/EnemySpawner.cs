using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    struct SpawnerEnemyStruct
    {
        public Enemy enemy;
        [Range(0, 100)] public int probability;
    }

    [Header("Parameters")]
    [SerializeField] private Enemy[] possibleEnemies;

    [Header("Public Infos")]
    [HideInInspector] public BattleTile associatedTile;

    
    public Enemy GetSpawnedEnemy(int dangerAmountToFill)
    {
        int antiCrashCounter = 0;

        while(antiCrashCounter++ < 20)
        {
            int pickedIndex = Random.Range(0, possibleEnemies.Length);

            if(possibleEnemies[pickedIndex].EnemyData.dangerLevel <  dangerAmountToFill) 
                return possibleEnemies[pickedIndex];
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
