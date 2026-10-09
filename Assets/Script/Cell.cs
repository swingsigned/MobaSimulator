using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Vector2 position;
    [SerializeField] private bool isAvailable = false;
    [SerializeField] private bool isTeamA = false;
    [SerializeField] private TerrainType type;
    [SerializeField] private readonly List<Champion> champions = new List<Champion>();
    public TerrainType Type
    {
        get => type;
        set => type = value;
    }
    public Vector2 Position
    {
        set => position = value;
        get => position;
    }
    public bool IsAvailable
    {
        set => isAvailable = value;
        get => isAvailable;
    }
    public bool IsTeamA
    {
        get => isTeamA;
        set => isTeamA = value;
    }
    public List<Champion> GetChampList()
    {
        return champions;
    }
    public void AddNewChamp(Champion champion)
    {
        champions.Add(champion);
    }
}
