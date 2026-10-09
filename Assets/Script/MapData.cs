using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public enum TerrainType
{
    Grass, Water, Ice, Land, HasTurretLand, SubNexus, MainNexus
}
[System.Serializable]
public class SpecialCellData
{
    public List<Vector2Int> positions = new();
    public GameObject terrainPrefab;
}

[CreateAssetMenu(menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public List<SpecialCellData> specialCells = new();
    public Dictionary<Vector2Int, GameObject> InitializeLookup()
    {
        var lookup = new Dictionary<Vector2Int, GameObject>();

        foreach (var cell in specialCells)
        {
            foreach (var position in cell.positions)
            {
                if (!lookup.TryAdd(position, cell.terrainPrefab))
                {
                    Debug.LogError(
                        $"Duplicate position in map data: {position}",
                        this
                    );
                }
            }
        }

        return lookup;
    }
}
