using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class GenProTile
{
    public Vector2Int tileCoordinates;
    public Room tileRoom;
    public GenProTile previousTile;
}

public class GenProPathCalculator 
{
    public GenProTile[,] floorGenProTiles;

    public GenProPathCalculator(int algoTabSize)
    {
        floorGenProTiles = new GenProTile[algoTabSize, algoTabSize];

        for(int x = 0; x < algoTabSize; x++)
        {
            for (int y = 0; y < algoTabSize; y++)
            {
                floorGenProTiles[x, y] = new GenProTile();
                floorGenProTiles[x, y].tileCoordinates = new Vector2Int(x, y);
            }
        }
    }

    public void AddRoom(Room room, Vector2Int location)
    {
        for(int x = 0; x < room.RoomSize.x; x++)
        {
            for (int y = 0; y < room.RoomSize.y; y++)
            {
                floorGenProTiles[location.x + x, location.y + y].tileRoom = room;
            }
        }
    }

    public List<Vector2Int> GetPath(Vector2Int startLocation, Vector2Int endLocation, bool includeStart = false, bool includeEnd = false)
    {
        List<Vector2Int> openList = new List<Vector2Int>();
        List<Vector2Int> closedList = new List<Vector2Int>();

        openList.Add(startLocation);

        int antiCrashCounter = 0;
        while(antiCrashCounter++ < 100 && openList.Count > 0)
        {
            int bestDist = 10000;
            int pickedIndex = 0;
            for(int i = 0; i < openList.Count; i++)
            {
                int currentDist = GetManhattanDistance(endLocation, openList[i]);
                currentDist += GetManhattanDistance(startLocation, openList[i]);

                if(currentDist < bestDist)
                {
                    bestDist = currentDist;
                    pickedIndex = i;    
                }
            }

            Vector2Int pickedPos = openList[pickedIndex];
            openList.RemoveAt(pickedIndex);
            closedList.Add(pickedPos);

            if(pickedPos == endLocation)
            {
                return GetFinalPath(startLocation, endLocation, includeStart, includeEnd);
            }

            List<Vector2Int> pickedLocationNeighbors = GetPossibleNeighborLocations(pickedPos);
            for(int i = 0; i < pickedLocationNeighbors.Count; i++)
            {
                if (openList.Contains(pickedLocationNeighbors[i])) continue;
                if (closedList.Contains(pickedLocationNeighbors[i])) continue;

                openList.Add(pickedLocationNeighbors[i]);
                floorGenProTiles[pickedLocationNeighbors[i].x, pickedLocationNeighbors[i].y].previousTile = 
                    floorGenProTiles[pickedPos.x, pickedPos.y];
            }
        }

        return new List<Vector2Int>();
    }

    private List<Vector2Int> GetFinalPath(Vector2Int startLocation, Vector2Int endLocation, bool includeStart = false, bool includeEnd = false)
    {
        List<Vector2Int> finalPath = new List<Vector2Int>();
        GenProTile currentTile = floorGenProTiles[endLocation.x, endLocation.y];
        currentTile = currentTile.previousTile;

        if (includeEnd) finalPath.Add(endLocation);

        while(currentTile.tileCoordinates != startLocation)
        {
            finalPath.Add(currentTile.tileCoordinates);

            currentTile = currentTile.previousTile;
        }

        if (includeStart) finalPath.Add(startLocation);

        finalPath.Reverse();
        return finalPath;
    }


    
    public int GetManhattanDistance(Vector2Int point1, Vector2Int point2)
    {
        return Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y);
    }


    private List<Vector2Int> GetPossibleNeighborLocations(Vector2Int startLocation)
    {
        List<Vector2Int> returnedList = new List<Vector2Int>();

        // Left
        if(VerifyNeighborTile(startLocation, startLocation + new Vector2Int(-1, 0)))
        {
            returnedList.Add(startLocation + new Vector2Int(-1, 0));
        }

        // Right
        if (VerifyNeighborTile(startLocation, startLocation + new Vector2Int(1, 0)))
        {
            returnedList.Add(startLocation + new Vector2Int(1, 0));
        }

        // Up
        if (VerifyNeighborTile(startLocation, startLocation + new Vector2Int(0, 1)))
        {
            returnedList.Add(startLocation + new Vector2Int(0, 1));
        }

        // Down
        if (VerifyNeighborTile(startLocation, startLocation + new Vector2Int(0, -1)))
        {
            returnedList.Add(startLocation + new Vector2Int(0, -1));
        }

        return returnedList;
    }

    private bool VerifyNeighborTile(Vector2Int startLocation, Vector2Int verifiedLocation)
    {
        // If both tiles are empty
        if (floorGenProTiles[startLocation.x, startLocation.y].tileRoom == null && 
            floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom == null)
        {
            return true;
        }

        // If only the base tile is not empty
        if (floorGenProTiles[startLocation.x, startLocation.y].tileRoom != null && 
            floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom == null)
        {
            if (floorGenProTiles[startLocation.x, startLocation.y].tileRoom.VerifyHasDoorToward(verifiedLocation))
            {
                return true;
            }
        }

        // If only the checked tile is not empty
        if (floorGenProTiles[startLocation.x, startLocation.y].tileRoom == null &&
            floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom != null)
        {
            if (floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom.VerifyHasDoorToward(startLocation))
            {
                return true;
            }
        }

        // If both tiles have a room on it
        if (floorGenProTiles[startLocation.x, startLocation.y].tileRoom != null &&
            floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom != null)
        {
            if(floorGenProTiles[startLocation.x, startLocation.y].tileRoom == floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom)
            {
                return true;
            }

            if (floorGenProTiles[verifiedLocation.x, verifiedLocation.y].tileRoom.VerifyHasDoorToward(startLocation) &&
                floorGenProTiles[startLocation.x, startLocation.y].tileRoom.VerifyHasDoorToward(verifiedLocation))
            {
                return true;
            }
        }

        return false;
    }
}
