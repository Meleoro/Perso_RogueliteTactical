using UnityEngine;

public class RoomGlobalCollider : MonoBehaviour
{
    [Header("Private Infos")]
    private Room associatedRoom;

    public void Setup(Room room)
    {
        associatedRoom = room;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hero"))
        {
            UIManager.Instance.Minimap.EnterRoom(associatedRoom.RoomCoordinates);
        }
    }
}
