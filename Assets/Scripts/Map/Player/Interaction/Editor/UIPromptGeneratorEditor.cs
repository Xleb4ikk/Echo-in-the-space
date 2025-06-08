#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIPromptGenerator))]
public class UIPromptGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        UIPromptGenerator generator = (UIPromptGenerator)target;
        
        EditorGUILayout.Space(10);
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = Color.green;
        buttonStyle.fixedHeight = 40;
        
        if (GUILayout.Button("СОЗДАТЬ UI ПОДСКАЗОК", buttonStyle))
        {
            generator.GeneratePromptUI();
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif 