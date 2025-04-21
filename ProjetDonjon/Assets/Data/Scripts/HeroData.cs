using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroData")]
public class HeroData : ScriptableObject
{
    [Header("Main Stats")]
    public int baseHealth;
    public int baseStrength;
    public int baseSpeed;
    public int baseLuck;

    [Header("Prefabs")]
    public Inventory heroInventoryPrefab;
}
