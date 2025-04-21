using UnityEngine;

public class RoomClosableEntrance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] gameObjectsToDestory;

    public void ActivateBlockableEntrance()
    {
        for(int i = 0; i < gameObjectsToDestory.Length; i++)
        {
            Destroy(gameObjectsToDestory[i]);
        }
    }
}
