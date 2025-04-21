using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



[Serializable]
public struct RoomEntrance
{
    public int entranceWidth;
    public Vector2Int entranceDirection;
}

[Serializable]
public struct RoomBlockableEntranceStruct
{
    public RoomClosableEntrance entrance;
    public Vector2Int towardEntranceDir;
    public int entranceWidth;
}


public class Room : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private BattleTile battleTilePrefab;
    [SerializeField] private Vector3 battleTilesOffset;

    [Header("Private Infos")]
    private Vector2Int roomCoordinates;
    private Vector2Int roomSize;
    private Vector2Int roomsSizeUnits;
    private RoomEntrance[] roomEntrances;
    private List<BattleTile> battleTiles;

    [Header("Public Infos")]
    public Vector2Int RoomCoordinates { get { return roomCoordinates; } }
    public Vector2Int RoomSize { get { return roomSize; } }
    public RoomEntrance[] RoomEntrances { get { return roomEntrances; } }

    [Header("References")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _battleGroundTilemap;
    [SerializeField] private Tilemap _wallsTilemap;
    [SerializeField] private Tilemap _bottomWallsTilemap;
    [SerializeField] private RoomBlockableEntranceStruct[] _blockableEntrances;

    [Header("Other References")]
    public Transform _heroSpawnerTr;


    private void Start()
    {
        SetupBattleTiles();
    }


    #region Gen Pro Functions

    public void SetupRoom(Vector2Int roomCoordinates, Vector2Int roomsSizeUnits)
    {
        this.roomCoordinates = roomCoordinates;
        this.roomsSizeUnits = roomsSizeUnits;
        CreateRoomEntrances();
        roomSize = CalculateRoomSize();
    }

    public void CloseUnusedEntrances(List<Room> neighbors)
    {
        for(int i = 0; i < roomEntrances.Length; i++)
        {
            bool closeDoor = true;
            Vector2Int roomTowardPosition = roomEntrances[i].entranceDirection + roomCoordinates;

            for (int j = 0; j < neighbors.Count; j++)
            {
                if (!neighbors[j].VerifyRoomIsOnCoordinate(roomTowardPosition)) continue;

                if (!neighbors[j].VerifyHasDoorToward(roomCoordinates)) continue;

                closeDoor = false;
            }

            if (closeDoor)
            {
                _blockableEntrances[i].entrance.gameObject.SetActive(true);
                _blockableEntrances[i].entrance.ActivateBlockableEntrance();
            }
        }
    }

    private Vector2Int CalculateRoomSize()
    {
        Vector2Int bottomLeftCoordinates = new Vector2Int(100000, 100000);
        Vector2Int upRightCoordinates = new Vector2Int(-100000, -100000);

        for (int x = _groundTilemap.cellBounds.min.x; x <= _groundTilemap.cellBounds.max.x; x++)
        {
            for (int y = _groundTilemap.cellBounds.min.y; y <= _groundTilemap.cellBounds.max.y; y++)
            {
                if (!_groundTilemap.HasTile(new Vector3Int(x, y))) continue;

                if(bottomLeftCoordinates.x > x) bottomLeftCoordinates.x = x;    
                if(bottomLeftCoordinates.y > y) bottomLeftCoordinates.y = y;

                if (upRightCoordinates.x < x) upRightCoordinates.x = x;
                if (upRightCoordinates.y < y) upRightCoordinates.y = y;
            }
        }

        int roomWidth = Mathf.Abs(upRightCoordinates.x - bottomLeftCoordinates.x) + 1;
        int roomHeight = Mathf.Abs(upRightCoordinates.y - bottomLeftCoordinates.y) + 1;


        return new Vector2Int(roomWidth / roomsSizeUnits.x, roomHeight / roomsSizeUnits.y);
    }

    private void CreateRoomEntrances()
    {
        roomEntrances = new RoomEntrance[_blockableEntrances.Length];   

        for(int i = 0; i < _blockableEntrances.Length; i++)
        {
            RoomEntrance newEntrance = new RoomEntrance();
            newEntrance.entranceWidth = _blockableEntrances[i].entranceWidth;
            newEntrance.entranceDirection = _blockableEntrances[i].towardEntranceDir;
            roomEntrances[i] = (newEntrance);
        }

        /*// Left & Right Entrances
        for(int y = bottomWallsTilemap.cellBounds.min.y; y <= wallsTilemap.cellBounds.max.y; y++)
        {
            if (!wallsTilemap.HasTile(new Vector3Int(wallsTilemap.cellBounds.min.x, y)) && 
                !bottomWallsTilemap.HasTile(new Vector3Int(wallsTilemap.cellBounds.min.x, y)) &&
                groundTilemap.HasTile(new Vector3Int(wallsTilemap.cellBounds.min.x, y)))
            {
                int currentY = (y / wallsTilemap.cellBounds.max.y) * roomSize.y;
                AddRoomEntrance(new Vector2Int(-1, currentY));
            }
            if (!wallsTilemap.HasTile(new Vector3Int(wallsTilemap.cellBounds.max.x, y)) &&
                !bottomWallsTilemap.HasTile(new Vector3Int(wallsTilemap.cellBounds.max.x, y)) &&
                groundTilemap.HasTile(new Vector3Int(wallsTilemap.cellBounds.max.x, y)))
            {
                int currentY = (y / wallsTilemap.cellBounds.max.y) * roomSize.y;
                AddRoomEntrance(new Vector2Int(1, currentY));
            }
        }

        // Up & Down Entrances
        for (int x = wallsTilemap.cellBounds.min.x; x <= wallsTilemap.cellBounds.max.x; x++)
        {
            if (!wallsTilemap.HasTile(new Vector3Int(x, wallsTilemap.cellBounds.min.y)) &&
                !bottomWallsTilemap.HasTile(new Vector3Int(x, wallsTilemap.cellBounds.min.y)) &&
                groundTilemap.HasTile(new Vector3Int(x, wallsTilemap.cellBounds.min.y)))
            {
                int currentX = (x / wallsTilemap.cellBounds.max.x) * roomSize.x;
                AddRoomEntrance(new Vector2Int(currentX, -1));
            }
            if (!wallsTilemap.HasTile(new Vector3Int(x, wallsTilemap.cellBounds.max.y)) &&
                !bottomWallsTilemap.HasTile(new Vector3Int(x, wallsTilemap.cellBounds.max.y)) &&
                groundTilemap.HasTile(new Vector3Int(x, wallsTilemap.cellBounds.max.y)))
            {
                int currentX = (x / wallsTilemap.cellBounds.max.x) * roomSize.x;
                AddRoomEntrance(new Vector2Int(currentX, 1));
            }
        }*/
    }

    /*private void AddRoomEntrance(Vector2Int direction)
    {
        bool found = false;

        for (int i = 0; i < roomEntrances.Count; i++)
        {
            if (roomEntrances[i].entranceDirection == direction)
            {
                RoomEntrance entrance = roomEntrances[i];
                entrance.entranceWidth += 1;
                roomEntrances[i] = entrance;
                found = true;
            }
        }

        if (!found)
        {
            RoomEntrance newEntrance = new RoomEntrance();
            newEntrance.entranceWidth = 1;
            newEntrance.entranceDirection = direction;
            roomEntrances.Add(newEntrance);
        }
    }*/

    public bool VerifyRoomsCompatibility(Room otherRoom)
    {
        // Verifies the rooms dont overlap
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.x; y++)
            {
                if (otherRoom.VerifyRoomIsOnCoordinate(new Vector2Int(roomCoordinates.x + x, roomCoordinates.y + y))) return false;
            }
        }
        
        // Verifies the rooms have a common entrance point
        for (int i = 0; i < roomEntrances.Length; i++)
        {
            Vector2Int roomTowardPosition = roomEntrances[i].entranceDirection + roomCoordinates;
            if (!otherRoom.VerifyRoomIsOnCoordinate(roomTowardPosition)) continue;

            for(int j = 0; j < otherRoom.RoomEntrances.Length; j++)
            {
                Vector2Int otherRoomTowardPosition = otherRoom.RoomEntrances[j].entranceDirection + otherRoom.RoomCoordinates;
                if (!VerifyRoomIsOnCoordinate(otherRoomTowardPosition)) continue;

                return true;
            }
        }

        return false;
    }

    public bool VerifyRoomFitsPath(Vector2Int roomPos, List<Vector2Int> path)
    {
        int roomPathIndex = -1;
        int previousRoomIndex = -1;
        int nextRoomIndex = -1;

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == roomPos)
            {
                roomPathIndex = i;
                if (i != 0) previousRoomIndex = i - 1;
            }

            if(roomPathIndex != -1 && nextRoomIndex == -1)
            {
                if(!VerifyRoomIsOnCoordinate(path[i])) 
                {
                    nextRoomIndex = i;
                }
            }
        }

        if (nextRoomIndex != -1)
        {
            if (!VerifyHasDoorToward(path[nextRoomIndex])) return false;
        }
        if(previousRoomIndex != -1)
        {
            if (!VerifyHasDoorToward(path[previousRoomIndex])) return false;
        }

        return true;
    }

    public bool VerifyRoomIsOnCoordinate(Vector2Int checkedCoordinate)
    {
        for(int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.x; y++)
            {
                if(checkedCoordinate == new Vector2Int(roomCoordinates.x + x, roomCoordinates.y + y)) return true;
            }
        }

        return false;
    }

    public bool VerifyHasDoorToward(Vector2Int checkedTowardPosition)
    {
        for (int i = 0; i < roomEntrances.Length; i++)
        {
            Vector2Int roomTowardPosition = roomEntrances[i].entranceDirection + roomCoordinates;
            if (checkedTowardPosition == roomTowardPosition) return true;
        }

        return false;
    }

    #endregion


    #region Battle Functions

    public void SetupBattleTiles()
    {
        for(int x = _battleGroundTilemap.cellBounds.min.x; x < _battleGroundTilemap.cellBounds.max.x; x++)
        {
            for (int y = _battleGroundTilemap.cellBounds.min.y; y < _battleGroundTilemap.cellBounds.max.y; y++)
            {
                if (!_battleGroundTilemap.HasTile(new Vector3Int(x, y))) continue;

                Vector3 tilePosWorld = _battleGroundTilemap.CellToWorld(new Vector3Int(x, y));
                BattleTile newTile = Instantiate(battleTilePrefab, tilePosWorld + battleTilesOffset, Quaternion.Euler(0, 0, 0), transform);

                newTile.SetupBattleTile(new Vector2Int(x, y));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hero"))
        {

        }
    }

    #endregion
}
