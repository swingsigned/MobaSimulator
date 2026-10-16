using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectOnCellData", menuName = "Scriptable Objects/ObjectOnCellData")]
public class ObjectOnCellData : ScriptableObject
{
    public enum ObjectTypeOnCell
    {
        NULL, Turret, Nexus, SubNexus, Tree
    }
    [System.Serializable]
    public class ObjectCell
    {
        public GameObject objBlueprint;
        public ObjectTypeOnCell objectType;
    }
    public List<ObjectCell> objectFieldMap = new();
    public ObjectCell GetObjectCell(ObjectTypeOnCell type)
    {
        return objectFieldMap.Find(t => t.objectType == type);
    }
}

