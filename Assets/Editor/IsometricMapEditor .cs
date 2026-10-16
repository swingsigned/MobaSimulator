
using UnityEditor;
using UnityEngine;

public class IsometricMapEditor : EditorWindow
{
    private MapData mapData;
    private TerrainData terrainData;

    private TerrainType selectedTerrain = TerrainType.Grass;
    private global::ObjectOnCellData.ObjectTypeOnCell selectedObjectType = ObjectOnCellData.ObjectTypeOnCell.NULL;
    private ObjectOnCellData objectOnCellData;
    private bool eraseMode;
    private int mapWidth = 0;
    private int mapHeight = 0;

    // Kích thước một ô trên màn hình Editor.
    private float cellWidth = 32f;
    private float cellHeight = 32f;
    float heightTriangle = 10;
    private int turnLeft = 1;

    private Vector2 scrollPosition;

    [MenuItem("Tools/Map Editor/Isometric Map Editor")]
    public static void Open()
    {
        GetWindow<IsometricMapEditor>("Isometric Map");
    }

    private void OnGUI()
    {

        DrawSettings();

        if (mapData == null || terrainData == null || objectOnCellData == null)
        {
            EditorGUILayout.HelpBox(
                "Hãy gán MapData, TerrainData, ObjectData để bắt đầu.",
                MessageType.Info
            );
            return;
        }

        DrawPaletteTerrain();
        EditorGUILayout.Space(6);
        DrawPaletteObject();
        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(
                $"Cells: {mapData.cells.Count}",
                GUILayout.Width(100)
            );

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save", GUILayout.Width(80)))
            {
                SaveMap();
            }
        }

        EditorGUILayout.Space(4);

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition
        );

        Rect canvasRect = GUILayoutUtility.GetRect(
            Mathf.Max(position.width - 30, 300),
            650,
            GUILayout.ExpandWidth(true)
        );

        DrawMap(canvasRect);

        EditorGUILayout.EndScrollView();
    }
    private void LoadMapSettings()
    {
        mapWidth = mapData.numCol;
        mapHeight = mapData.numRow;
        turnLeft = mapData.turnLeft;

        Repaint();
    }
    private void DrawSettings()
    {
        EditorGUILayout.LabelField(
            "Map Settings",
            EditorStyles.boldLabel
        );
        EditorGUI.BeginChangeCheck();

        MapData newMapData = (MapData)EditorGUILayout.ObjectField(
            "Map Data",
            mapData,
            typeof(MapData),
            false
        );

        if (EditorGUI.EndChangeCheck())
        {
            mapData = newMapData;

            if (mapData != null)
            {
                LoadMapSettings();
            }
        }
        terrainData = (TerrainData)EditorGUILayout.ObjectField(
            "Terrain Data",
            terrainData,
            typeof(TerrainData),
            false
        );
        objectOnCellData = (ObjectOnCellData)EditorGUILayout.ObjectField(
            "Object Data",
            objectOnCellData,
            typeof(ObjectOnCellData),
            false
        );
        mapWidth = EditorGUILayout.IntSlider(
            "Grid Width", mapWidth, 1, 32
        );
        mapHeight = EditorGUILayout.IntSlider(
            "Grid Height", mapHeight, 1, 32
        );
        cellWidth = EditorGUILayout.Slider(
            "Cell Width", cellWidth, 10f, 64f
        );

        cellHeight = EditorGUILayout.Slider(
            "Cell Height", cellHeight, 10f, 64f
        );

        heightTriangle = EditorGUILayout.Slider(
            "Height Triangle", heightTriangle, 0, 32
        );
        turnLeft = EditorGUILayout.IntField("Turn Left", turnLeft);
        eraseMode = EditorGUILayout.Toggle("Erase Mode", eraseMode);
    }

    private void DrawPaletteTerrain()
    {
        EditorGUILayout.LabelField(
            "Terrain Palette",
            EditorStyles.boldLabel
        );

        if (terrainData.terrains.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "TerrainData chưa có Terrain nào.",
                MessageType.Warning
            );
            return;
        }

        foreach (var terrain in terrainData.terrains)
        {
            if (terrain == null)
                continue;

            bool selected =
                selectedTerrain == terrain.terrainType;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !eraseMode;

                if (GUILayout.Toggle(
                    selected,
                    terrain.terrainType.ToString(),
                    "Button",
                    GUILayout.Width(150)))
                {
                    selectedTerrain = terrain.terrainType;
                }

                GUI.enabled = true;

                EditorGUILayout.ObjectField(
                    terrain.terrainPrefab,
                    typeof(GameObject),
                    false
                );
            }
        }
    }
    private void DrawPaletteObject()
    {
        EditorGUILayout.LabelField(
            "Object Palette",
            EditorStyles.boldLabel
        );

        if (objectOnCellData.objectFieldMap.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Object chưa có cái nào.",
                MessageType.Warning
            );
            return;
        }

        foreach (var object_ in objectOnCellData.objectFieldMap)
        {
            if (object_ == null)
                continue;

            bool selected =
                selectedObjectType == object_.objectType;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !eraseMode;

                if (GUILayout.Toggle(
                    selected,
                    object_.objectType.ToString(),
                    "Button",
                    GUILayout.Width(150)))
                {
                    selectedObjectType = object_.objectType;
                }

                GUI.enabled = true;

                EditorGUILayout.ObjectField(
                    object_.objBlueprint,
                    typeof(GameObject),
                    false
                );
            }
        }
    }

    private void DrawMap(Rect rect)
    {
        EditorGUI.DrawRect(
            rect,
            new Color(0.16f, 0.16f, 0.16f)
        );


        Vector2 corner = new Vector2(
            rect.x + (rect.width - (cellWidth * mapWidth)) / 2,
            rect.y + (rect.height - (cellHeight * mapHeight)) / 2
        );

        Event e = Event.current;
        int midCol = (int)Mathf.Floor(mapWidth / 2f);
        int maxColToRemove = mapHeight / 2;
        // Debug.Log(maxColToRemove);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // int numColToremove = Mathf.Abs(y % (2 * maxColToRemove) - maxColToRemove); //3, 2
                // // Debug.Log(numColToremove);
                // int numColOfRow = mapWidth - numColToremove; //6, 7
                // int numIndexNeedFallBack = (int)Mathf.Floor(numColOfRow / 2); //3, 4
                // int minIndexRange = midCol - numIndexNeedFallBack; //1, 0
                // int maxIndexRange = numColOfRow + minIndexRange - 1; // 6, 6
                int minIndexRange = 0;
                int maxIndexRange = mapWidth;
                if (x >= minIndexRange && x <= maxIndexRange)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    Vector2 screen = GridToScreen(position, corner);

                    Rect cellRect = new Rect(
                        screen.x,
                        screen.y,
                        cellWidth,
                        cellHeight
                    );
                    TerrainType? type = GetTerrainType(position);
                    global::ObjectOnCellData.ObjectTypeOnCell? objectTypeOnCell = GetObjectType(position);
                    Color color = type.HasValue
                        ? GetTerrainColor(type.Value)
                        : new Color(0.3f, 0.3f, 0.3f, 0.35f);
                    Color color1 = type.HasValue ? GetObjectColor(objectTypeOnCell.Value) :
                    new Color(0f, 0f, 0f, 0.35f);
                    DrawCircleCell(cellRect, color1);
                    DrawHexCell(cellRect, color);

                    // Viền của ô.
                    Vector3[] points =
                    {
                    new Vector3(cellRect.x , cellRect.y + heightTriangle),
                    new Vector3(cellRect.x +  cellRect.width / 2,  cellRect.y),
                    new Vector3(cellRect.x + cellRect.width, cellRect.y + heightTriangle),
                    new Vector3(cellRect.x + cellRect.width, cellRect.y + (cellRect.height - heightTriangle)),
                    new Vector3(cellRect.x +  cellRect.width / 2, cellRect.y + cellRect.height),
                    new Vector3(cellRect.x, cellRect.y + (cellRect.height - heightTriangle)),
                    new Vector3(cellRect.x , cellRect.y + heightTriangle)
                };

                    Handles.color = color;
                    Handles.DrawAAConvexPolygon(points);

                    // Hiển thị tọa độ.
                    GUI.Label(
                        cellRect,
                        $"{x},{y}",
                        new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal =
                            {
                            textColor = Color.white
                            }
                        }
                    );
                    //Chuột trái 
                    if (e.type == EventType.MouseDown &&
                        e.button == 0 &&
                        cellRect.Contains(e.mousePosition))
                    {
                        PaintCell(position);
                        e.Use();
                    }
                    if (e.type == EventType.MouseDrag &&
                        e.button == 0 &&
                        cellRect.Contains(e.mousePosition))
                    {
                        PaintCell(position);
                        e.Use();
                    }
                    //Chuột phải 
                    if (e.type == EventType.MouseDown &&
                       e.button == 1 &&
                       cellRect.Contains(e.mousePosition))
                    {
                        PaintObjectCell(position);
                        e.Use();
                    }
                    if (e.type == EventType.MouseDrag &&
                        e.button == 1 &&
                        cellRect.Contains(e.mousePosition))
                    {
                        PaintObjectCell(position);
                        e.Use();
                    }
                }
            }
        }

        if (e.type == EventType.MouseUp)
            Repaint();
    }
    private Vector2 GridToScreen(
        Vector2Int position,
        Vector2 corner)
    {
        float X = corner.x + (position.x * cellWidth);
        float Y = corner.y + (position.y * cellHeight);
        if (position.y % 2 != 0)
        {
            return new Vector2(X - (cellWidth / 2) * turnLeft, Y);
        }
        return new Vector2(X, Y);
    }

    private void DrawCell(Rect rect, Color color)
    {
        EditorGUI.DrawRect(rect, color);
    }
    //Thằng này vẽ hình luc giac
    private void DrawHexCell(Rect corner, Color color)
    {
        Vector3[] points =
        {
            new Vector3(corner.x , corner.y + heightTriangle),
            new Vector3(corner.x +  corner.width / 2,  corner.y),
            new Vector3(corner.x + corner.width, corner.y + heightTriangle),
            new Vector3(corner.x + corner.width, corner.y + (corner.height - heightTriangle)),
            new Vector3(corner.x +  corner.width / 2, corner.y + corner.height),
            new Vector3(corner.x, corner.y + (corner.height - heightTriangle)),
            new Vector3(corner.x , corner.y + heightTriangle)
        };

        Handles.color = color;
        Handles.DrawAAConvexPolygon(points);
    }
    private void DrawCircleCell(Rect cell, Color color)
    {
        Vector3 center = new Vector3(
            cell.x + cell.width / 2f,
            cell.y + cell.height / 2f,
            0f
        );

        float radius = Mathf.Min(cell.width, cell.height) / 2f;

        Handles.color = color;
        Handles.DrawSolidDisc(center, Vector3.forward, radius);
    }
    private TerrainType? GetTerrainType(Vector2Int position)
    {
        MapCellData cell = mapData.cells.Find(
            c => c.position == position
        );

        return cell == null ? null : cell.terrainType;
    }
    private global::ObjectOnCellData.ObjectTypeOnCell? GetObjectType(Vector2Int position)
    {
        MapCellData cell = mapData.cells.Find(
            c => c.position == position
        );

        return cell == null ? null : cell.objectType;
    }
    //Đặt màu
    private Color GetTerrainColor(TerrainType type)
    {
        return type switch
        {
            TerrainType.Grass => new Color(0.25f, 0.55f, 0.25f),
            TerrainType.Water => new Color(0.15f, 0.4f, 0.85f),
            TerrainType.Ice => new Color(0.55f, 0.85f, 1f),
            TerrainType.Land => new Color(0.55f, 0.42f, 0.28f),
            TerrainType.HasTurretLand => new Color(0.85f, 0.55f, 0.2f),
            TerrainType.SubNexus => new Color(0.7f, 0.3f, 0.8f),
            TerrainType.MainNexus => new Color(1f, 0.2f, 0.2f),
            _ => new Color(0.25f, 0.55f, 0.25f)
        };
    }
    private Color GetObjectColor(global::ObjectOnCellData.ObjectTypeOnCell type)
    {
        return type switch
        {
            ObjectOnCellData.ObjectTypeOnCell.Tree => Color.yellow,
            _ => Color.black
        };
    }

    private void PaintCell(Vector2Int position)
    {
        Undo.RecordObject(mapData, "Edit Isometric Map");

        if (eraseMode)
        {
            mapData.RemoveTerrainCell(position);
        }
        else
        {
            mapData.SetCell(position, selectedTerrain);
        }

        EditorUtility.SetDirty(mapData);
        Repaint();
    }
    private void PaintObjectCell(Vector2Int position)
    {
        Undo.RecordObject(mapData, "Edit Isometric Map");

        if (eraseMode)
        {
            mapData.RemoveObjectCell(position);
        }
        else
        {
            mapData.SetObjectCell(position, selectedObjectType);
        }

        EditorUtility.SetDirty(mapData);
        Repaint();
    }
    private void SaveMap()
    {
        mapData.numCol = mapWidth;
        mapData.numRow = mapHeight;
        mapData.turnLeft = turnLeft;
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();

        Debug.Log("MapData saved.", mapData);
    }
}