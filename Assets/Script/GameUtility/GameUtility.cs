using System.Collections.Generic;
using Unity.VisualScripting;
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
    static public bool IsCellselected = false;
    static public bool hasChampSelected = false;
    static public List<Champion> curChamps; //Demo
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
    public static bool checkIfMouseHit<T>(Vector2 mousePosition, out Collider2D colHit)
    {
        Vector2 worldPoint = changeScreenPointToWorldPoint(mousePosition);
        var hit = Physics2D.OverlapPoint(worldPoint);
        if (hit != null && hit.GetComponentInParent<T>() != null)
        {
            colHit = hit;
            return true;
        }
        colHit = null;
        return false;
    }
}
