using UnityEngine;

public static class GameUtility
{
    //For Cell
    public static Vector2 cellSize = new Vector2(128, 96);
    //For Map
    public static int numRow = 9;
    public static int numCol = 7;
    public static float PPU = cellSize.y / 2;
    static public int minRow = 6, maxRow = 9;
    static public bool Isselected = false;
    public static Vector2 changeGridToWorld(int XinGrid, int YinGrid)
    {
        return Vector2.zero;
    }
    public static Vector2 changeWorld2Grid(int X, int Y)
    {
        return Vector2.zero;
    }
    public static Vector2 changeScreenPointToWorldPoint(Vector2 screenPoint)
    {
        Vector3 screenPointV3 = screenPoint;
        return Camera.main.ScreenToWorldPoint(screenPointV3);
    }
    public static Collider2D checkIfMouseHit(Vector2 mousePosition)
    {
        Vector2 worldPoint = GameUtility.changeScreenPointToWorldPoint(mousePosition);
        return Physics2D.OverlapPoint(worldPoint);

    }
}
