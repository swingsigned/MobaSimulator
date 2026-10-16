using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class MapController : MonoBehaviour
{

    [SerializeField] private Dictionary<Vector2, Cell> mapUnderMatrixForm;
    [SerializeField] private Champion demoChampion;
    [SerializeField] private GameObject cellDefault;
    [SerializeField] private TerrainData terrainData;
    [SerializeField] private ObjectOnCellData objectOnCellData;
    [SerializeField] private MapData mapData;
    private Dictionary<Vector2Int, MapCellData> mapLookup;
    public void OnEnable()
    {
        mapLookup = mapData.InitializeLookup();
    }
    public void SpawnCell()
    {
        mapUnderMatrixForm = new Dictionary<Vector2, Cell>();
        int numRow = mapData.numRow;
        int numCol = mapData.numCol;


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
                SpawnTerrainOnCell(col, row, out Cell cell);
                SpawnObjectOnCell(col, row);
                var cellVisualSprite = cell.GetComponent<SpriteRenderer>();
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
                cell.Position = new Vector2(col, row);
                mapUnderMatrixForm.TryAdd(cell.Position, cell.GetComponentInChildren<Cell>());

            }
        }
    }
    public void SpawnTerrainOnCell(int col, int row, out Cell cell)
    {
        float PPU = GameUtility.PPU;
        Vector2 cellSize = GameUtility.cellSize;
        float offsetX = cellSize.x / PPU;
        float offsetY = cellSize.y / PPU;
        //Compute Spawn Location
        Vector2 spawnPosition = new Vector2(col * offsetX, -(row * offsetY - (row * (23 / PPU))));
        float designedOffset = Mathf.Max(cellSize.x, cellSize.y) / PPU / 2;
        if (row % 2 != 0)
        {
            spawnPosition.x -= designedOffset * mapData.turnLeft;
        }
        //Find Terrain and Spawn cell
        Terrain terrainAtCell = new Terrain();
        terrainAtCell.terrainPrefab = cellDefault;
        terrainAtCell.terrainType = TerrainType.Grass;
        if (mapLookup.TryGetValue(new Vector2Int(col, row), out MapCellData cellData))
        {
            terrainAtCell = terrainData.GetTerrain(cellData.terrainType);
        }
        var cellBlueprint = terrainAtCell.terrainPrefab;
        var iCell = Instantiate(cellBlueprint, spawnPosition + (Vector2)transform.position, Quaternion.identity, transform);
        cell = iCell.GetComponent<Cell>();
        cell.GetComponentInChildren<SpriteRenderer>().sortingOrder = row;
        if (cell.Type == TerrainType.Water)
        {
            cell.GetComponentInChildren<SpriteRenderer>().sortingOrder = -1;
        }
    }

    public void SpawnObjectOnCell(int col, int row)
    {
        float PPU = GameUtility.PPU;
        Vector2 cellSize = GameUtility.cellSize;
        float offsetX = cellSize.x / PPU;
        float offsetY = cellSize.y / PPU;
        //Compute Spawn Location
        Vector2 spawnPosition = new Vector2(col * offsetX, -(row * offsetY - (row * (23 / PPU))));
        spawnPosition.y += (110 / 84 / 2);
        float designedOffset = Mathf.Max(cellSize.x, cellSize.y) / PPU / 2;
        if (row % 2 != 0)
        {
            spawnPosition.x -= designedOffset * mapData.turnLeft;
        }
        //Find Object and Spawn cell
        ObjectOnCellData.ObjectCell objectAtCell = new ObjectOnCellData.ObjectCell();
        objectAtCell.objectType = ObjectOnCellData.ObjectTypeOnCell.NULL;
        if (mapLookup.TryGetValue(new Vector2Int(col, row), out MapCellData cellData))
        {
            objectAtCell = objectOnCellData.GetObjectCell(cellData.objectType);
        }
        if (objectAtCell.objectType == ObjectOnCellData.ObjectTypeOnCell.NULL)
        {
            return;
        }
        var objectBlueprint = objectAtCell.objBlueprint;
        var iObject = Instantiate(objectBlueprint, spawnPosition + (Vector2)transform.position, Quaternion.identity, transform);
        iObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = row;

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
