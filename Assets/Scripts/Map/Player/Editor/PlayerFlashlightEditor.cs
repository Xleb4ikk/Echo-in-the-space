#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerFlashlight))]
public class PlayerFlashlightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        PlayerFlashlight flashlight = (PlayerFlashlight)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Инструменты тестирования", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Показать подсказку\nиспользования", GUILayout.Height(40)))
        {
            flashlight.ForceShowPrompt();
            EditorUtility.SetDirty(flashlight);
        }
        
        if (GUILayout.Button("Сбросить состояние\nфонарика", GUILayout.Height(40)))
        {
            flashlight.ResetFlashlightState();
            EditorUtility.SetDirty(flashlight);
        }
        EditorGUILayout.EndHorizontal();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Тестирование подсказок работает только во время игры.", MessageType.Info);
        }
        else if (UIPromptManager.Instance == null)
        {
            EditorGUILayout.HelpBox("UIPromptManager не найден в сцене! Подсказки не будут отображаться.", MessageType.Error);
        }
    }
}
#endif 