using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class MapController : MonoBehaviour
{

    [SerializeField] private Dictionary<Vector2, Cell> mapUnderMatrixForm;
    [SerializeField] private Champion demoChampion;
    [SerializeField] private GameObject cellDefault;
    [SerializeField] private TerrainData terrainData;
    [SerializeField] private MapData mapData;
    public void SpawnCell()
    {
        var lookup = mapData.InitializeLookup();
        mapUnderMatrixForm = new Dictionary<Vector2, Cell>();
        int numRow = mapData.numRow;
        int numCol = mapData.numCol;
        float PPU = GameUtility.PPU;
        Vector2 cellSize = GameUtility.cellSize;

        int midCol = (int)Mathf.Floor(numCol / 2f);
        Debug.Log("Mid col la:" + midCol);
        int maxColToRemove = numRow / 2;
        for (int row = 0; row < numRow; row++)
        {
            for (int col = 0; col < numCol; col++)
            {
                // int isEvenCol = j % 2;
                // int numColToremove = Mathf.Abs(row % (2 * maxColToRemove) - maxColToRemove); //3, 2
                // int numColOfRow = numCol - numColToremove; //6, 7
                // int numIndexNeedFallBack = (int)Mathf.Floor(numColOfRow / 2); //3, 4
                // int minIndexRange = midCol - numIndexNeedFallBack; //1, 0
                // int maxIndexRange = numColOfRow + minIndexRange - 1; // 6, 6

                // Debug.Log(minIndexRange + $",{col}," + maxIndexRange);
                float offsetX = cellSize.x / PPU;
                float offsetY = cellSize.y / PPU;
                Vector2 position = new Vector2(col * offsetX, -(row * offsetY - (row * (23 / PPU))));
                float designedOffset = Mathf.Max(cellSize.x, cellSize.y) / PPU / 2;
                if (row % 2 != 0)
                {
                    position.x -= designedOffset * mapData.turnLeft;
                }
                Terrain terrainAtCell = new Terrain();
                terrainAtCell.terrainPrefab = cellDefault;
                terrainAtCell.terrainType = TerrainType.Grass;
                if (lookup.TryGetValue(new Vector2Int(col, row), out TerrainType terrainType))
                {
                    terrainAtCell = terrainData.GetTerrain(terrainType);
                }
                var cellBlueprint = terrainAtCell.terrainPrefab;
                var iCell = Instantiate(cellBlueprint, position + (Vector2)transform.position, Quaternion.identity, transform);
                var cell = iCell.GetComponent<Cell>();
                var cellVisualSprite = iCell.GetComponentInChildren<SpriteRenderer>();
                cellVisualSprite.sortingOrder = row;
                if (col < midCol)
                {
                    if (cell.Type == TerrainType.HasTurretLand)
                    {
                        cellVisualSprite.flipX = true;
                        Vector2 offsetXforTurret = cell.transform.GetChild(0).localPosition;
                        cell.transform.GetChild(0).localPosition = new Vector2(offsetXforTurret.x * -1, offsetXforTurret.y);
                    }
                    if (cell.Type == TerrainType.Land)
                    {
                        // cellVisualSprite.color = GameUtility.redTeamCellColor;
                    }

                }
                if (cell.Type == TerrainType.Water)
                {
                    cellVisualSprite.sortingOrder = -1;
                }
                cell.Position = new Vector2(col, row);
                mapUnderMatrixForm.TryAdd(cell.Position, iCell.GetComponentInChildren<Cell>());

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
        SpawnCell();
        // var cell = getCellByPositionOnMatrix(demoChampion.PositionByCell);
        // Debug.Log(cell);
        // demoChampion.transform.position = cell.transform.position;
        // cell.AddNewChamp(demoChampion);
    }
}
