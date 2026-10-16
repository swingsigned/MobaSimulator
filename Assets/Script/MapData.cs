
using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
[Serializable]
public class MapCellData
{
    public Vector2Int position;
    public TerrainType terrainType;
    public ObjectOnCellData.ObjectTypeOnCell objectType;
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
    public Dictionary<Vector2Int, MapCellData> InitializeLookup()
    {
        var lookup = new Dictionary<Vector2Int, MapCellData>();

        foreach (var cell in cells)
        {
            if (!lookup.TryAdd(cell.position, cell))
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
    public void SetObjectCell(Vector2Int position, ObjectOnCellData.ObjectTypeOnCell objectType)
    {
        MapCellData cell = cells.Find(c => c.position == position);

        if (cell != null)
        {
            cell.objectType = objectType;
            return;
        }

        cells.Add(new MapCellData
        {
            position = position,
            objectType = objectType
        });
    }
    public bool RemoveTerrainCell(Vector2Int position)
    {
        MapCellData cell = cells.Find(c => c.position == position);

        if (cell == null)
            return false;

        cell.terrainType = TerrainType.Grass;
        return true;
    }
    public bool RemoveObjectCell(Vector2Int position)
    {
        MapCellData cell = cells.Find(c => c.position == position);

        if (cell == null)
            return false;

        cell.objectType = ObjectOnCellData.ObjectTypeOnCell.NULL;
        return true;
    }
}