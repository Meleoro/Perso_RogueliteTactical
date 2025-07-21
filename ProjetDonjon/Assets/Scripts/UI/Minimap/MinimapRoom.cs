using UnityEngine;
using UnityEngine.UI;

public class MinimapRoom : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Sprite[] possibleIcons;
    [SerializeField] private Color displayedColor;
    [SerializeField] private Color visitedColor;

    [Header("Public Infos")]
    public RectTransform RectTr { get { return _rectTr; } }
    public Vector2Int Coodinates { get { return coordinates; } }

    [Header("Private Infos")]
    private Vector2Int coordinates;
    private MinimapRoom[] neighborRooms;
    private bool isDisplayed;
    private bool isVisited;

    [Header("References")]
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Image _roomIconImage;
    [SerializeField] private Image[] _allImages;
    [SerializeField] private Image[] _roomEntrancesImages;
    [SerializeField] private GameObject _roomEntrancesParent;


    #region Setup

    public void SetupRoom(Vector2Int coordinates)
    {
        this.coordinates = coordinates;

        _roomEntrancesParent.SetActive(false);
        for (int i = 0; i < _allImages.Length; i++)
        {
            _allImages[i].enabled = false;
        }
    }

    public void SetupNeighborsAndEntrances(MinimapRoom[] neighbors)
    {
        neighborRooms = neighbors;

        for (int i = 0; i < neighbors.Length; i++)
        {
            Vector2Int coordDif = neighbors[i].coordinates - coordinates;

            if (coordDif.y > 0) _roomEntrancesImages[0].enabled = true;
            else if (coordDif.x > 0) _roomEntrancesImages[1].enabled = true;
            else if (coordDif.y < 0) _roomEntrancesImages[2].enabled = true;
            else if (coordDif.x < 0) _roomEntrancesImages[3].enabled = true;

        }
    }

    #endregion


    #region Others

    public void EnterRoom()
    {
        if (isVisited) return;

        isDisplayed = true;
        isVisited = true;
        _allImages[0].enabled = true;

        _roomEntrancesParent.SetActive(true);

        foreach (MinimapRoom neighbor in  neighborRooms)
        {
            neighbor.EnterNeighborRoom();
        }

        foreach (Image image in _allImages)
        {
            image.color = visitedColor;
        }
    }

    public void EnterNeighborRoom()
    {
        if (isDisplayed) return;

        isDisplayed = true;
        _allImages[0].enabled = true;

        foreach (Image image in _allImages)
        {
            image.color = displayedColor;
        }
    }

    #endregion
}
