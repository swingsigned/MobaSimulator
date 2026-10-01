using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private bool isAvailable;
    [SerializeField] private bool isTeamA;
    public int X
    {
        set => x = value;
        get => x;
    }
    public int Y
    {
        set => y = value;
        get => y;
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
}
