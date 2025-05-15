using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerationManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private EnviroData enviroData;
    [SerializeField] private Vector2Int roomSizeUnits;
    [SerializeField] private Vector2 offsetRoomCenter;


    [Header("Private Infos")]
    private int wantedRoomAmount;
    private List<Room> generatedRooms;
    private Vector3 spawnPos;

    [Header("References")]
    [SerializeField] private HeroesManager _heroesManager;
    private GenProPathCalculator _pathCalculator;


    private void Start()
    {
        GenerateFloor(enviroData);
        _heroesManager.Initialise(null, spawnPos);
    }


    public void GenerateFloor(EnviroData enviroData)
    {
        generatedRooms = new List<Room>();

        this.enviroData = enviroData;
        wantedRoomAmount = Random.Range(enviroData.minRoomAmount, enviroData.maxRoomAmount);

        int tabSize = wantedRoomAmount * 2;
        _pathCalculator = new GenProPathCalculator(tabSize);

        GenerateStartAndEnd(new Vector2Int(wantedRoomAmount, wantedRoomAmount));
        CloseUnusedEntrances();
    }


    private void GenerateStartAndEnd(Vector2Int centerPosition)
    {
        // Start
        AddRoom(centerPosition, enviroData.possibleStartRooms[Random.Range(0, enviroData.possibleStartRooms.Length)]);
        spawnPos = generatedRooms[0]._heroSpawnerTr.position;

        // End
        Vector2Int endPos = new Vector2Int(0, 0);
        int antiCrashCounter = 0;
        while (_pathCalculator.GetManhattanDistance(centerPosition, endPos) > enviroData.maxDistGoldenPath ||
            _pathCalculator.GetManhattanDistance(centerPosition, endPos) < enviroData.minDistGoldenPath && ++antiCrashCounter < 1000)
        {
            endPos = new Vector2Int(centerPosition.x + Random.Range(-enviroData.minDistGoldenPath, enviroData.minDistGoldenPath),
                centerPosition.y + Random.Range(2, enviroData.maxDistGoldenPath));
        }

        AddRoom(endPos, enviroData.possibleStairsRooms[Random.Range(0, enviroData.possibleStairsRooms.Length)]);
        GeneratePath(centerPosition, endPos);
    }


    private void CloseUnusedEntrances()
    {
        for(int i = 0; i < generatedRooms.Count; i++)
        {
            generatedRooms[i].CloseUnusedEntrances(GetNeighborRooms(generatedRooms[i].RoomCoordinates, generatedRooms[i]));
        }
    }


    #region Utility Functions


    private void GeneratePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = _pathCalculator.GetPath(start, end);
        int antiCrashCounter = 0;

        while(!VerifyPathIsValid(path) && ++antiCrashCounter < 1000)
        {
            Room testedRoom = enviroData.possibleBattleRooms[Random.Range(0, enviroData.possibleBattleRooms.Length)];
            Vector2Int currentCoordinates = path[0];
            bool found = false;

            for (int i = 0; i < path.Count; i++)
            {
                if (_pathCalculator.floorGenProTiles[path[i].x, path[i].y].tileRoom != null) continue;
                testedRoom.SetupRoom(path[i], roomSizeUnits);

                if (!VerifyRoomFits(path[i], testedRoom)) continue;
                if (!testedRoom.VerifyRoomFitsPath(path[i], path)) continue;

                currentCoordinates = path[i];
                found = true;
                break;
            }

            if (found)
            {
                AddRoom(currentCoordinates, testedRoom);
            }
        }
    }


    private void AddRoom(Vector2Int spawnCoordinates, Room room)
    {
        Room newRoom = Instantiate(room, (Vector2)(spawnCoordinates * roomSizeUnits * new Vector2(2, 1.5f)) + offsetRoomCenter, Quaternion.Euler(0, 0, 0),
            transform);
        newRoom.SetupRoom(spawnCoordinates, roomSizeUnits);
        _pathCalculator.AddRoom(newRoom, spawnCoordinates);
        generatedRooms.Add(newRoom);
    }


    private bool VerifyPathIsValid(List<Vector2Int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (_pathCalculator.floorGenProTiles[path[i].x, path[i].y].tileRoom == null) return false;
        }
        return true;
    }


    private bool VerifyRoomFits(Vector2Int coordinates, Room room)
    {
        List<Room> neighborRooms = GetNeighborRooms(coordinates, room);

        for (int i = 0; i < neighborRooms.Count; i++)
        {
            if (!room.VerifyRoomsCompatibility(neighborRooms[i])) return false;
        }

        return true;
    }

    private List<Room> GetNeighborRooms(Vector2Int coordinates, Room room)
    {
        List<Room> neighborRooms = new List<Room>();

        // We verify if the room doesnt overlap another and we get the neighbor rooms
        for (int x = 0; x < room.RoomSize.x; x++)
        {
            for (int y = 0; y < room.RoomSize.y; y++)
            {
                Vector2Int currentCoordinates = new Vector2Int(coordinates.x + x, coordinates.y + y);

                //if (_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y].tileRoom != null) continue;

                if (_pathCalculator.floorGenProTiles[currentCoordinates.x + 1, currentCoordinates.y].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x + 1, currentCoordinates.y].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x + 1, currentCoordinates.y].tileRoom);
                    }
                }
                if (_pathCalculator.floorGenProTiles[currentCoordinates.x - 1, currentCoordinates.y].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x - 1, currentCoordinates.y].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x - 1, currentCoordinates.y].tileRoom);
                    }
                }

                if (_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y + 1].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y + 1].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y + 1].tileRoom);
                    }
                }

                if (_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y - 1].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y - 1].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y - 1].tileRoom);
                    }
                }
            }
        }

        return neighborRooms;
    }


    #endregion


    private void GenerateSpecialRooms()
    {

    }
}
