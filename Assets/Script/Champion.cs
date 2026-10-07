using UnityEngine;

public class Champion : MonoBehaviour
{
    [SerializeField] private Vector2 positionByCell;
    [SerializeField] private string nameChamp;
    public Vector2 PositionByCell
    {
        get => positionByCell;
        set => positionByCell = value;
    }
    public string Name
    {
        get => nameChamp;
        set => nameChamp = value;
    }
}
