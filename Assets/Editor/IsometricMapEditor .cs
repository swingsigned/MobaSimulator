
using UnityEditor;
using UnityEngine;

public class IsometricMapEditor : EditorWindow
{
    private MapData mapData;
    private TerrainData terrainData;

    private TerrainType selectedTerrain = TerrainType.Grass;
    private bool eraseMode;
    private int mapWidth = GameUtility.numCol;
    private int mapHeight = GameUtility.numRow;

    // Kích thước một ô trên màn hình Editor.
    private float cellWidth = 64f;
    private float cellHeight = 64f;
    float heightTriangle = 14;
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

        if (mapData == null || terrainData == null)
        {
            EditorGUILayout.HelpBox(
                "Hãy gán MapData và TerrainData để bắt đầu.",
                MessageType.Info
            );
            return;
        }

        DrawPalette();

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

    private void DrawSettings()
    {
        EditorGUILayout.LabelField(
            "Map Settings",
            EditorStyles.boldLabel
        );

        mapData = (MapData)EditorGUILayout.ObjectField(
            "Map Data",
            mapData,
            typeof(MapData),
            false
        );

        terrainData = (TerrainData)EditorGUILayout.ObjectField(
            "Terrain Data",
            terrainData,
            typeof(TerrainData),
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

    private void DrawPalette()
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
                int numColToremove = Mathf.Abs(y % (2 * maxColToRemove) - maxColToRemove); //3, 2
                // Debug.Log(numColToremove);
                int numColOfRow = mapWidth - numColToremove; //6, 7
                int numIndexNeedFallBack = (int)Mathf.Floor(numColOfRow / 2); //3, 4
                int minIndexRange = midCol - numIndexNeedFallBack; //1, 0
                int maxIndexRange = numColOfRow + minIndexRange - 1; // 6, 6
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

                    TerrainType? type = GetTerrainAt(position);

                    Color color = type.HasValue
                        ? GetTerrainColor(type.Value)
                        : new Color(0.3f, 0.3f, 0.3f, 0.35f);

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

    private TerrainType? GetTerrainAt(Vector2Int position)
    {
        MapCellData cell = mapData.cells.Find(
            c => c.position == position
        );

        return cell == null ? null : cell.terrainType;
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
            _ => Color.gray
        };
    }

    private void PaintCell(Vector2Int position)
    {
        Undo.RecordObject(mapData, "Edit Isometric Map");

        if (eraseMode)
        {
            mapData.RemoveCell(position);
        }
        else
        {
            mapData.SetCell(position, selectedTerrain);
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