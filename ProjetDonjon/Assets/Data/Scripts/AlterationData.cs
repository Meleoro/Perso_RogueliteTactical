using UnityEngine;

public enum AlterationType
{
    Weakened,
    Strength,
    Vulnerable,
    Provocked,
    Hindered,
    Shield
}

[CreateAssetMenu(fileName = "AlterationData", menuName = "Scriptable Objects/AlterationData")]
public class AlterationData : ScriptableObject
{
    public string alterationName;
    public string alterationDescription;
    public AlterationType alterationType;

    public bool isPositive;
    public bool isInfinite;
    public int duration;
    public int strength;
}
