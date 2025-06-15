using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoPathVisualizer))]
public class AutoPathVisualizerEditor : Editor
{
    private void OnSceneGUI()
    {
        AutoPathVisualizer visualizer = (AutoPathVisualizer)target;

        Transform parent = visualizer.transform;
        int count = parent.childCount;
        if (count < 2) return;

        Handles.color = visualizer.lineColor;

        for (int i = 0; i < count; i++)
        {
            Transform point = parent.GetChild(i);
            if (point == null) continue;

            // Перемещаемый хэндл
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(point.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(point, "Move Path Point");
                point.position = newPos;
            }

            // Нарисовать линию к следующей точке
            if (i < count - 1)
            {
                Handles.DrawLine(point.position, parent.GetChild(i + 1).position);
            }
            else if (visualizer.loop && count > 2)
            {
                Handles.DrawLine(point.position, parent.GetChild(0).position);
            }

            // Подпись и кликабельная точка
            Handles.Label(point.position + Vector3.up * 0.5f, $"Point {i}");

            if (Handles.Button(point.position + Vector3.up * 0.3f, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap))
            {
                Selection.activeTransform = point;
            }
        }
    }
}
