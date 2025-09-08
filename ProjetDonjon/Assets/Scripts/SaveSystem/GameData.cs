using UnityEngine;

public class GameData 
{
    // HEROES
    public int[] heroesLevel;
    public bool[] heroesUnlockedNodes;
    public int[] heroesCurrentSkillPoint;
    public string[] heroesEquippedSkillIndexes;
    public string[] heroesEquippedPassiveIndexes;

    // EQUIPMENT / RELICS
    public bool[] possessedRelicsIndexes;



    public GameData()
    {
        heroesLevel = new int[4];
        heroesUnlockedNodes = new bool[4 * 15];
        heroesCurrentSkillPoint = new int[4];
        heroesEquippedSkillIndexes = new string[4 * 6];
        heroesEquippedPassiveIndexes = new string[4 * 3];

        possessedRelicsIndexes = new bool[12 * 4];
    }
}
