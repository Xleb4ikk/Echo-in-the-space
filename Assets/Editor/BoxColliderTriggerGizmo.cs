using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class BoxColliderTriggerGizmo
{
    private static bool showGizmos = true;
    private static bool useColors = true;

    private static readonly Color[] colors = new Color[]
    {
        new Color(0f, 1f, 0f, 0.15f),     // €рко-зелЄный
        new Color(1f, 0f, 0f, 0.15f),     // €рко-красный
        new Color(1f, 1f, 0f, 0.15f),     // жЄлтый
        new Color(1f, 0f, 1f, 0.15f),     // €рко-розовый (магента)
        new Color(0f, 1f, 1f, 0.15f),     // €рко-голубой (циан)
        new Color(0.5f, 1f, 0.5f, 0.15f), // светло-зелЄный
        new Color(1f, 0.5f, 0.5f, 0.15f), // светло-красный
    };

    private static readonly Color[] textColors = new Color[]
    {
        Color.green,
        Color.red,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        new Color(0f, 0.5f, 0f),   // тЄмно-зелЄный, но не чЄрный
        new Color(0.5f, 0f, 0f),   // тЄмно-красный
    };


    static BoxColliderTriggerGizmo()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        showGizmos = EditorPrefs.GetBool("BoxColliderTriggerGizmo_Show", true);
        useColors = EditorPrefs.GetBool("BoxColliderTriggerGizmo_UseColors", true);
    }

    [MenuItem("Tools/Toggle BoxCollider Trigger Gizmos %#g")]
    private static void ToggleShow()
    {
        showGizmos = !showGizmos;
        EditorPrefs.SetBool("BoxColliderTriggerGizmo_Show", showGizmos);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Toggle BoxCollider Trigger Gizmos", true)]
    private static bool ToggleShowValidate()
    {
        Menu.SetChecked("Tools/Toggle BoxCollider Trigger Gizmos", showGizmos);
        return true;
    }

    [MenuItem("Tools/Toggle BoxCollider Trigger Colors")]
    private static void ToggleColors()
    {
        useColors = !useColors;
        EditorPrefs.SetBool("BoxColliderTriggerGizmo_UseColors", useColors);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Toggle BoxCollider Trigger Colors", true)]
    private static bool ToggleColorsValidate()
    {
        Menu.SetChecked("Tools/Toggle BoxCollider Trigger Colors", useColors);
        return true;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!showGizmos) return;

        BoxCollider[] colliders = Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);

        int colorIndex = 0;

        foreach (var col in colliders)
        {
            if (col != null && col.isTrigger)
            {
                Color fillColor = useColors ? colors[colorIndex % colors.Length] : new Color(0f, 1f, 0f, 0.15f);
                Color outlineColor = fillColor;
                outlineColor.a = 0.9f;

                Color labelColor = useColors ? textColors[colorIndex % textColors.Length] : Color.green;

                DrawBoxColliderGizmo(col, fillColor, outlineColor, labelColor);

                colorIndex++;
            }
        }
    }

    private static void DrawBoxColliderGizmo(BoxCollider col, Color fillColor, Color outlineColor, Color labelColor)
    {
        Transform t = col.transform;
        Matrix4x4 oldMatrix = Handles.matrix;
        Handles.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

        Vector3 center = col.center;
        Vector3 size = col.size;

        Handles.DrawSolidRectangleWithOutline(GetBoxFace(center, size, Vector3.forward), fillColor, outlineColor);
        Handles.DrawSolidRectangleWithOutline(GetBoxFace(center, size, Vector3.back), fillColor, outlineColor);
        Handles.DrawSolidRectangleWithOutline(GetBoxFace(center, size, Vector3.left), fillColor, outlineColor);
        Handles.DrawSolidRectangleWithOutline(GetBoxFace(center, size, Vector3.right), fillColor, outlineColor);
        Handles.DrawSolidRectangleWithOutline(GetBoxFace(center, size, Vector3.up), fillColor, outlineColor);
        Handles.DrawSolidRectangleWithOutline(GetBoxFace(center, size, Vector3.down), fillColor, outlineColor);

        Handles.matrix = oldMatrix;

        Vector3 labelPos = t.position + t.rotation * col.center + Vector3.up * 0.5f * Mathf.Max(col.size.x, col.size.y, col.size.z);
        Handles.color = labelColor;
        Handles.Label(labelPos, col.gameObject.name);

        // ѕровер€ем, выделен ли объект
        if (Selection.activeGameObject == col.gameObject)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(t.position, t.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Move Trigger");
                t.position = newPosition;
            }
        }
    }

    private static Vector3[] GetBoxFace(Vector3 center, Vector3 size, Vector3 normal)
    {
        Vector3 half = size * 0.5f;

        Vector3 right = Vector3.right * half.x;
        Vector3 up = Vector3.up * half.y;
        Vector3 forward = Vector3.forward * half.z;

        if (normal == Vector3.forward)
        {
            return new Vector3[]
            {
                center + up + right + forward,
                center + up - right + forward,
                center - up - right + forward,
                center - up + right + forward
            };
        }
        else if (normal == Vector3.back)
        {
            return new Vector3[]
            {
                center + up + right - forward,
                center + up - right - forward,
                center - up - right - forward,
                center - up + right - forward
            };
        }
        else if (normal == Vector3.left)
        {
            return new Vector3[]
            {
                center + up - forward - right,
                center + up + forward - right,
                center - up + forward - right,
                center - up - forward - right
            };
        }
        else if (normal == Vector3.right)
        {
            return new Vector3[]
            {
                center + up - forward + right,
                center + up + forward + right,
                center - up + forward + right,
                center - up - forward + right
            };
        }
        else if (normal == Vector3.up)
        {
            return new Vector3[]
            {
                center + forward + right + up,
                center + forward - right + up,
                center - forward - right + up,
                center - forward + right + up
            };
        }
        else // Vector3.down
        {
            return new Vector3[]
            {
                center + forward + right - up,
                center + forward - right - up,
                center - forward - right - up,
                center - forward + right - up
            };
        }
    }
}
