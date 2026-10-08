using System.Collections.Generic;
using System.Linq.Expressions;
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
        int midCol = (int)Mathf.Floor(numCol / 2f);
        Debug.Log("Mid col la:" + midCol);
        int maxColToRemove = numCol - GameUtility.minCol;
        for (int row = 0; row < numRow; row++)
        {
            for (int col = 0; col < numCol; col++)
            {
                // int isEvenCol = j % 2;
                int numColToremove = Mathf.Abs(row % (2 * maxColToRemove) - maxColToRemove); //3, 2
                int numColOfRow = GameUtility.maxCol - numColToremove; //6, 7
                int numIndexNeedFallBack = (int)Mathf.Floor(numColOfRow / 2); //3, 4
                int minIndexRange = midCol - numIndexNeedFallBack; //1, 0
                int maxIndexRange = numColOfRow + minIndexRange - 1; // 6, 6
                if (col >= minIndexRange && col <= maxIndexRange)
                {
                    Debug.Log(minIndexRange + $",{col}," + maxIndexRange);
                    float offsetX = GameUtility.cellSize.x / GameUtility.PPU;
                    float offsetY = GameUtility.cellSize.y / GameUtility.PPU;
                    Vector2 position = new Vector2(col * offsetX, -(row * offsetY - (row * (15 / GameUtility.PPU))));
                    if (row % 2 != 0)
                    {
                        position.x -= 0.5f;
                    }
                    var Icell = Instantiate(cellBlueprint, position + (Vector2)transform.position, Quaternion.identity, transform);
                    var Cell = Icell.GetComponent<Cell>();
                    Cell.Position = new Vector2(col, row);
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
        // var cell = getCellByPositionOnMatrix(demoChampion.PositionByCell);
        // Debug.Log(cell);
        // demoChampion.transform.position = cell.transform.position;
        // cell.AddNewChamp(demoChampion);
    }
}
