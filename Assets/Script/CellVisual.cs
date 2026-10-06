using UnityEngine;

public class CellVisual : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selSprite;
    [SerializeField] private bool isSel;
    public void change2SelSprite()
    {
        if (!isSel && !GameUtility.Isselected)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = selSprite;
            isSel = true;
            GameUtility.Isselected = true;
        }
    }
    public void change2NorSprite()
    {
        if (isSel)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = normalSprite;
            isSel = false;
            GameUtility.Isselected = false;
        }
    }
    public void changeSprite()
    {
        if (!isSel)
            change2SelSprite();
        else change2NorSprite();
    }
}
