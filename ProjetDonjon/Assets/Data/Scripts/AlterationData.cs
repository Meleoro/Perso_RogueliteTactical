using UnityEngine;

public enum AlterationType
{
    Weakened,
    Strength,
    Vulnerable,
    Provocked,
    Hindered,
    Shield,
    Thorn,
    Lucky,
    Unlucky
}

[CreateAssetMenu(fileName = "AlterationData", menuName = "Scriptable Objects/AlterationData")]
public class AlterationData : ScriptableObject
{
    public string alterationName;
    public string alterationDescription;
    public Sprite alterationIcon;

    public AlterationType alterationType;

    public bool isPositive;
    public bool isInfinite;
    public bool isStackable;
    public int duration;
    public float strength;
}
