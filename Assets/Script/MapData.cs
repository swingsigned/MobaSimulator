
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapCellData
{
    public Vector2Int position;
    public TerrainType terrainType;
}

[CreateAssetMenu(
    fileName = "MapData",
    menuName = "Game/Map/MapData"
)]
public class MapData : ScriptableObject
{
    public List<MapCellData> cells = new();
    public int numCol;
    public int numRow;
    public int turnLeft = 1;
    public Dictionary<Vector2Int, TerrainType> InitializeLookup()
    {
        var lookup = new Dictionary<Vector2Int, TerrainType>();

        foreach (var cell in cells)
        {
            if (!lookup.TryAdd(cell.position, cell.terrainType))
            {
                Debug.LogError(
                    $"Duplicate map position: {cell.position}",
                    this
                );
            }
        }

        return lookup;
    }

    public void SetCell(Vector2Int position, TerrainType terrainType)
    {
        MapCellData cell = cells.Find(c => c.position == position);

        if (cell != null)
        {
            cell.terrainType = terrainType;
            return;
        }

        cells.Add(new MapCellData
        {
            position = position,
            terrainType = terrainType
        });
    }

    public bool RemoveCell(Vector2Int position)
    {
        int index = cells.FindIndex(c => c.position == position);

        if (index < 0)
            return false;

        cells.RemoveAt(index);
        return true;
    }
}