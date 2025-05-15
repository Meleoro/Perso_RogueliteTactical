using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Main Infos")]
    public string unitName;
    public Sprite unitImage;

    [Header("Main Stats")]
    public int baseHealth;
    public int baseStrength;
    public int baseSpeed;
    public int baseLuck;
}
