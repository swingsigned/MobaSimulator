using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private Dictionary<Cell, Vector2> mapUnderMatrixForm;
    [SerializeField] private GameObject prefabCell;
    public void SpawnCell(GameObject cellBlueprint)
    {
        mapUnderMatrixForm = new Dictionary<Cell, Vector2>();
        int numRow = GameUtility.numRow;
        int numCol = GameUtility.numCol;
        for (int i = 0; i < numRow; i++)
        {
            Vector2 offset = Vector2.zero;
            if (i % 2 == 0)
            {

            }
            for (int j = 0; j < numCol; j++)
            {
                Vector2 position = new Vector2(i, -j);
                var Icell = Instantiate(cellBlueprint, position, Quaternion.identity, transform);
                mapUnderMatrixForm.TryAdd(Icell.GetComponentInChildren<Cell>(), position);
            }
        }
    }
    void Start()
    {
        SpawnCell(prefabCell);
    }
}
