using UnityEngine;
using UnityEditor;

public class RenameObjectsEditor : EditorWindow
{
    string baseName = "Object_";
    int startIndex = 0;

    [MenuItem("Tools/Batch Rename Objects")]
    public static void ShowWindow()
    {
        GetWindow<RenameObjectsEditor>("Batch Renamer");
    }

    void OnGUI()
    {
        GUILayout.Label("Mass Rename Selected Objects", EditorStyles.boldLabel);
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        startIndex = EditorGUILayout.IntField("Start Index", startIndex);

        if (GUILayout.Button("Rename"))
        {
            RenameSelectedObjects();
        }
    }

    void RenameSelectedObjects()
    {
        var selected = Selection.gameObjects;

        // Сортировка по имени (по желанию можно поменять)
        System.Array.Sort(selected, (a, b) => a.name.CompareTo(b.name));

        for (int i = 0; i < selected.Length; i++)
        {
            selected[i].name = baseName + (startIndex + i);
        }
    }
}
