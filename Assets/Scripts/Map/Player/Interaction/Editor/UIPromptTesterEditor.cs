#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIPromptTester))]
public class UIPromptTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        UIPromptTester tester = (UIPromptTester)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Тестирование системы подсказок", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Показать\nПодсказку 1", GUILayout.Height(40)))
        {
            tester.ShowTestMessage1();
        }
        
        if (GUILayout.Button("Показать\nПодсказку 2", GUILayout.Height(40)))
        {
            tester.ShowTestMessage2();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Скрыть подсказку", GUILayout.Height(30)))
        {
            tester.HideMessage();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Запустить автоматический тест", GUILayout.Height(35)))
        {
            tester.StartAutoTest();
        }
        
        EditorGUILayout.HelpBox("Клавиши для теста:\n1 - Показать первую подсказку\n2 - Показать вторую подсказку\n0 - Скрыть подсказку", MessageType.Info);
    }
}
#endif 