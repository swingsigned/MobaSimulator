using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Vector2 position;
    [SerializeField] private bool isAvailable = false;
    [SerializeField] private bool isTeamA = false;
    [SerializeField]
    private
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

}
