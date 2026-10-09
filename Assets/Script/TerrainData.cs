using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    Grass, Water, Ice, Land, HasTurretLand, SubNexus, MainNexus
}
[System.Serializable]
public class Terrain
{
    public GameObject terrainPrefab;
    public TerrainType terrainType;
}
[CreateAssetMenu(fileName = "TerrainData", menuName = "Game/Map/TerrainData")]
public class TerrainData : ScriptableObject
{
    public List<Terrain> terrains = new();
    public Terrain GetTerrain(TerrainType type)
    {
        return terrains.Find(t => t.terrainType == type);
    }
}
