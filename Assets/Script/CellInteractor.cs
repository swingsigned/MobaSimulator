using UnityEngine;

public class CellInteractor : IInteractCell
{
    public void Interact(Collider2D hit)
    {
        if (!hit) return;
        Cell cell = hit.GetComponentInParent<Cell>();

        if (cell == null)
            return;

        CellVisual visual = hit.GetComponentInChildren<CellVisual>();

        if (visual == null)
            return;

        visual.changeSprite();

        Debug.Log($"Clicked Cell: {cell.Position}");
    }
}