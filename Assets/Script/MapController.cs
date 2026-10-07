using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private Dictionary<Vector2, Cell> mapUnderMatrixForm;
    [SerializeField] private GameObject prefabCell;
    [SerializeField] private Champion demoChampion;
    public void SpawnCell(GameObject cellBlueprint)
    {
        mapUnderMatrixForm = new Dictionary<Vector2, Cell>();
        int numRow = GameUtility.numRow;
        int numCol = GameUtility.numCol;
        int midRow = (int)Mathf.Floor(numRow / 2f);
        Debug.Log("Mid row la:" + midRow);
        int maxRowToRemove = numRow - GameUtility.minRow;
        for (int i = 0; i < numRow; i++)
        {
            for (int j = 0; j < numCol; j++)
            {
                // int isEvenCol = j % 2;
                int numRowToremove = Mathf.Abs(j % (2 * maxRowToRemove) - maxRowToRemove); //3, 2
                int numRowOfColumn = GameUtility.maxRow - numRowToremove; //6, 7
                int numIndexNeedFallBack = (int)Mathf.Floor(numRowOfColumn / 2); //3, 4
                int minIndexRange = midRow - numIndexNeedFallBack; //1, 0
                int maxIndexRange = numRowOfColumn + minIndexRange - 1; // 6, 6

                if (i >= minIndexRange && i <= maxIndexRange)
                {
                    float offsetX = GameUtility.cellSize.x / GameUtility.PPU;
                    float offsetY = GameUtility.cellSize.y / GameUtility.PPU;
                    Vector2 position = new Vector2(j * offsetX + j * (-19 / GameUtility.PPU), -i * offsetY);
                    if (j % 2 != 0)
                    {
                        position.y += 0.5f * 2;
                    }
                    var Icell = Instantiate(cellBlueprint, position + (Vector2)transform.position, Quaternion.identity, transform);
                    var Cell = Icell.GetComponent<Cell>();
                    Cell.Position = new Vector2(i, j);
                    mapUnderMatrixForm.TryAdd(Cell.Position, Icell.GetComponentInChildren<Cell>());
                }
            }
        }
    }
    public Cell getCellByPositionOnMatrix(Vector2 position)
    {
        mapUnderMatrixForm.TryGetValue(position, out Cell cell);
        if (cell == null) return null;
        return cell;
    }
    void Start()
    {
        SpawnCell(prefabCell);
        var cell = getCellByPositionOnMatrix(demoChampion.PositionByCell);
        Debug.Log(cell);
        demoChampion.transform.position = cell.transform.position;
        cell.AddNewChamp(demoChampion);
    }
}
