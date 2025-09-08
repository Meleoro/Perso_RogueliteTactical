using UnityEngine;

public interface ISaveable
{
    void LoadGame(GameData data);
    void SaveGame(ref GameData data);
}
