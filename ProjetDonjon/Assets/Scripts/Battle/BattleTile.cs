using UnityEngine;

public class BattleTile : MonoBehaviour
{
    [Header("Private Infos")]
    private Vector2Int tileCoordinates;
    private Unit unitOnTile;
    


    public void SetupBattleTile(Vector2Int tileCoordinates)
    {
        this.tileCoordinates = tileCoordinates;
    }

    public void UnitEnterTile(Unit unit)
    {
        unitOnTile = unit;
    }

    public void UnitLeaveTile(Unit unit)
    {
        unitOnTile = null;
    }
}
