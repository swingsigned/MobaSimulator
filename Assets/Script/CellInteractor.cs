using UnityEngine;

public class CellInteractor : IInteractCell
{
    MaterialPropertyBlock block;
    public void Interact(Collider2D hit)
    {
        block = new MaterialPropertyBlock();
        // if (!hit) return;
        // Cell cell = hit.GetComponentInParent<Cell>();

        // if (cell == null)
        //     return;

        // CellVisual visual = hit.GetComponentInChildren<CellVisual>();
        SetStrength(hit.GetComponent<SpriteRenderer>(), 1);
        // if (visual == null)
        //     return;

        // visual.changeSprite();

        // Debug.Log($"Clicked Cell: {cell.Position}");
    }
    void SetStrength(SpriteRenderer sr, float v)
    {
        sr.GetPropertyBlock(block);
        block.SetFloat(GameUtility.StrengthId, v);
        sr.SetPropertyBlock(block);
    }
}