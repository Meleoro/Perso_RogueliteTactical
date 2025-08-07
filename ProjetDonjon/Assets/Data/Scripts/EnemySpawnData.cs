using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnData", menuName = "Scriptable Objects/EnemySpawnData")]
public class EnemySpawnData : ScriptableObject
{
    public EnemySpawn[] possibleEnemies;
}

[Serializable]
public class EnemySpawn
{
    public Unit enemyPrefab;
    [Range(0, 100)] public int proba;
    [Range(0, 100)] public int eliteProba;
    public int maxCountPerBattle;
    public int minEnemyCountBeforeSpawn;
}
