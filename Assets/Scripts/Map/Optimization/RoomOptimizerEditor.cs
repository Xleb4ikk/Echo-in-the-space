#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(RoomOptimizer))]
public class RoomOptimizerEditor : Editor
{
    private RoomOptimizer optimizer;
    private SerializedProperty rooms;
    private SerializedProperty portalConnections;
    private SerializedProperty player;
    private SerializedProperty portalTriggerRadius;
    private SerializedProperty debugMode;
    
    private void OnEnable()
    {
        optimizer = (RoomOptimizer)target;
        rooms = serializedObject.FindProperty("rooms");
        portalConnections = serializedObject.FindProperty("portalConnections");
        player = serializedObject.FindProperty("player");
        portalTriggerRadius = serializedObject.FindProperty("portalTriggerRadius");
        debugMode = serializedObject.FindProperty("debugMode");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Получаем ссылку на компонент
        optimizer = (RoomOptimizer)target;
        
        // Отображаем базовые настройки
        DrawGeneralSettings();
        
        // Отображаем раздел настройки комнат
        DrawRoomsSection();
        
        // Отображаем раздел настройки порталов
        DrawConnectionsSection();
        
        // Отображаем кнопку для проверки источников света
        DrawLightCheckerButton();
        
        // Применяем изменения
        serializedObject.ApplyModifiedProperties();
        
        // Если что-то изменилось, помечаем объект как "грязный"
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    
    private void DrawGeneralSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Room Optimizer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This component manages the transition between rooms using a two-portal system.\n" +
                                "1. The Preparation Portal activates the destination room\n" +
                                "2. The Transition Portal deactivates the source room", MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(player);
        
        // Добавляем выбор начальной активной комнаты
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Initial Active Room", GUILayout.Width(120));
        int initialIndex = optimizer.initialActiveRoom != null ? optimizer.rooms.IndexOf(optimizer.initialActiveRoom) : -1;
        string[] roomNames = optimizer.rooms.Select(r => r != null ? r.roomName : "None").ToArray();
        int newIndex = EditorGUILayout.Popup(initialIndex, roomNames);
        if (newIndex != initialIndex && newIndex >= 0 && newIndex < optimizer.rooms.Count)
        {
            optimizer.initialActiveRoom = optimizer.rooms[newIndex];
            EditorUtility.SetDirty(optimizer);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.PropertyField(portalTriggerRadius);
        EditorGUILayout.PropertyField(debugMode);
    }
    
    private void DrawRoomsSection()
    {
        // Комнаты
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rooms", EditorStyles.boldLabel);
        
        // Кнопка для добавления новой комнаты
        if (GUILayout.Button("Add Room"))
        {
            rooms.arraySize++;
            SerializedProperty newRoom = rooms.GetArrayElementAtIndex(rooms.arraySize - 1);
            newRoom.FindPropertyRelative("roomName").stringValue = "Room " + rooms.arraySize;
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.Space();
        
        // Отображение и редактирование комнат
        for (int i = 0; i < rooms.arraySize; i++)
        {
            SerializedProperty room = rooms.GetArrayElementAtIndex(i);
            SerializedProperty roomName = room.FindPropertyRelative("roomName");
            SerializedProperty roomBounds = room.FindPropertyRelative("roomBounds");
            SerializedProperty roomLights = room.FindPropertyRelative("roomLights");
            SerializedProperty roomCenter = room.FindPropertyRelative("roomCenter");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Заголовок комнаты с возможностью удаления
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(roomName.stringValue, EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                rooms.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                break;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(roomName);
            EditorGUILayout.PropertyField(roomCenter);
            EditorGUILayout.PropertyField(roomBounds);
            
            // Источники света
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(roomLights);
            
            // Кнопка для автоматического поиска источников света
            if (GUILayout.Button("Detect Lights"))
            {
                if (roomCenter.objectReferenceValue != null)
                {
                    serializedObject.ApplyModifiedProperties();
                    
                    // Получаем комнату напрямую из списка оптимизатора
                    if (i < optimizer.rooms.Count)
                    {
                        optimizer.DetectRoomLights(optimizer.rooms[i]);
                    }
                    
                    serializedObject.Update();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Room center must be set before detecting lights", "OK");
                }
            }
            
            // После кнопки "Detect Lights" добавляем новые кнопки для работы с позицией комнаты
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Move Room to View", GUILayout.Width(150)))
            {
                if (roomCenter.objectReferenceValue != null)
                {
                    // Перемещаем центр комнаты в точку, на которую смотрит камера сцены
                    SceneView view = SceneView.lastActiveSceneView;
                    if (view != null)
                    {
                        Transform centerTransform = roomCenter.objectReferenceValue as Transform;
                        if (centerTransform != null)
                        {
                            // Сохраняем значение Y, чтобы комната не "прыгала" вверх-вниз
                            float yPos = centerTransform.position.y;
                            Vector3 viewPos = view.camera.transform.position + view.camera.transform.forward * 10f;
                            centerTransform.position = new Vector3(viewPos.x, yPos, viewPos.z);
                            EditorUtility.SetDirty(centerTransform);
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            if (GUILayout.Button("Align with Floor", GUILayout.Width(150)))
            {
                if (roomCenter.objectReferenceValue != null)
                {
                    Transform centerTransform = roomCenter.objectReferenceValue as Transform;
                    if (centerTransform != null)
                    {
                        // Выполняем рейкаст вниз для определения позиции пола
                        RaycastHit hit;
                        if (Physics.Raycast(centerTransform.position, Vector3.down, out hit, 100f))
                        {
                            // Use a default room height instead of trying to access localScale from SerializedProperty
                            float roomHeight = 3f; // Default room height
                            
                            if (roomBounds.objectReferenceValue != null)
                            {
                                Transform boundsTransform = roomBounds.objectReferenceValue as Transform;
                                if (boundsTransform != null)
                                {
                                    roomHeight = boundsTransform.localScale.y;
                                }
                            }
                            
                            centerTransform.position = new Vector3(
                                centerTransform.position.x,
                                hit.point.y + roomHeight * 0.5f,
                                centerTransform.position.z
                            );
                            EditorUtility.SetDirty(centerTransform);
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            // После блока с кнопками для работы с позицией комнаты добавляем кнопку для перемещения комнаты с содержимым
            if (roomCenter.objectReferenceValue != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Move Room with Contents", GUILayout.Width(180)))
                {
                    Transform centerTransform = roomCenter.objectReferenceValue as Transform;
                    if (centerTransform != null && i < optimizer.rooms.Count)
                    {
                        // Выбираем объект в сцене для перемещения
                        Selection.activeGameObject = centerTransform.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                        
                        // Показываем подсказку пользователю
                        EditorUtility.DisplayDialog("Перемещение комнаты", 
                            "Выбран центр комнаты. Используйте инструмент перемещения (W) в сцене и удерживайте Shift, чтобы переместить комнату с содержимым.", 
                            "OK");
                    }
                }
                
                if (GUILayout.Button("Move to Scene View", GUILayout.Width(150)))
                {
                    Transform centerTransform = roomCenter.objectReferenceValue as Transform;
                    if (centerTransform != null && i < optimizer.rooms.Count)
                    {
                        // Получаем позицию в центре вида сцены
                        SceneView view = SceneView.lastActiveSceneView;
                        if (view != null)
                        {
                            Vector3 viewPos = view.camera.transform.position + view.camera.transform.forward * 10f;
                            
                            // Сохраняем высоту комнаты
                            viewPos.y = centerTransform.position.y;
                            
                            // Вызываем специальный метод для перемещения комнаты вместе с содержимым
                            Undo.RecordObject(optimizer, "Move Room to Scene View");
                            optimizer.MoveRoom(optimizer.rooms[i], viewPos);
                            EditorUtility.SetDirty(optimizer);
                            
                            // Обновляем окно сцены
                            SceneView.RepaintAll();
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }

            // Добавляем пользовательские ручки для манипуляции комнатой в сцене
            if (roomCenter.objectReferenceValue != null && Selection.activeGameObject == (roomCenter.objectReferenceValue as Transform).gameObject)
            {
                EditorGUILayout.HelpBox("Комната выбрана в сцене. Используйте стандартные инструменты перемещения (W) для изменения позиции.", MessageType.Info);
            }
            else if (roomCenter.objectReferenceValue != null)
            {
                if (GUILayout.Button("Select Room in Scene"))
                {
                    Transform centerTransform = roomCenter.objectReferenceValue as Transform;
                    if (centerTransform != null)
                    {
                        Selection.activeGameObject = centerTransform.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
            }
            
            // Стены комнаты
            EditorGUILayout.Space();
            
            // Кнопка для создания центра комнаты
            EditorGUILayout.Space();
            if (roomCenter.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create Room Center"))
                {
                    GameObject centerObj = new GameObject($"{roomName.stringValue}_Center");
                    centerObj.transform.SetParent(optimizer.transform);
                    
                    // Если это первая комната, размещаем ее в начале координат
                    if (i == 0)
                    {
                        centerObj.transform.position = Vector3.zero;
                    }
                    else
                    {
                        // Для других комнат - смещаем от предыдущей
                        SerializedProperty prevRoom = rooms.GetArrayElementAtIndex(i-1);
                        SerializedProperty prevCenter = prevRoom.FindPropertyRelative("roomCenter");
                        if (prevCenter.objectReferenceValue != null)
                        {
                            Transform prevTransform = (prevCenter.objectReferenceValue as Transform);
                            centerObj.transform.position = prevTransform.position + new Vector3(15f, 0f, 0f);
                        }
                        else
                        {
                            centerObj.transform.position = new Vector3(i * 15f, 0f, 0f);
                        }
                    }
                    
                    roomCenter.objectReferenceValue = centerObj.transform;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
    
    private void DrawConnectionsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Portal Connections", EditorStyles.boldLabel);
        
        // Добавляем пояснение, как работают порталы
        EditorGUILayout.HelpBox("Для каждого соединения между комнатами настройте два портала:\n" +
                              "1. Подготовительный портал (Prep Portal) - активирует свет в целевой комнате\n" +
                              "2. Переходной портал (Trans Portal) - выключает свет в исходной комнате", 
                              MessageType.Info);
        
        // Отображение списка соединений
        EditorGUILayout.PropertyField(portalConnections);
        
        // Кнопка для настройки коллайдеров всех порталов
        if (GUILayout.Button("Setup All Portal Colliders", GUILayout.Height(30)))
        {
            serializedObject.ApplyModifiedProperties();
            
            foreach (var connection in optimizer.portalConnections)
            {
                optimizer.SetupPortalColliders(connection);
            }
            
            serializedObject.Update();
            EditorUtility.SetDirty(target);
        }
        
        // Кнопка для добавления нового соединения
        if (GUILayout.Button("Add Portal Connection"))
        {
            // Добавляем новое соединение
            int connectionIndex = portalConnections.arraySize;
            portalConnections.arraySize++;
            
            // Применяем изменения и работаем напрямую с объектом
            serializedObject.ApplyModifiedProperties();
            
            // Создаем новое соединение напрямую с объектом RoomOptimizer
            if (optimizer.rooms.Count >= 2)
            {
                optimizer.portalConnections[connectionIndex].connectionName = "Connection " + (connectionIndex + 1);
                optimizer.portalConnections[connectionIndex].sourceRoom = optimizer.rooms[0];
                optimizer.portalConnections[connectionIndex].destinationRoom = optimizer.rooms[1];
            }
            
            // Обновляем serializedObject
            serializedObject.Update();
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.Space();
        
        // Кнопка для автоматического создания соединений
        if (optimizer.rooms.Count >= 2)
        {
            if (GUILayout.Button("Auto-Generate Portal Connections", GUILayout.Height(30)))
            {
                serializedObject.ApplyModifiedProperties();
                
                // Создаем соединения между комнатами
                AutoGenerateConnections();
                
                serializedObject.Update();
                EditorUtility.SetDirty(target);
            }
        }
    }

    private void DrawLightCheckerButton()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Maintenance Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check & Fix All Lights"))
        {
            optimizer.CheckAndFixLights();
        }
        
        if (GUILayout.Button("Show Instructions"))
        {
            optimizer.ShowInstructions();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Enable Debug Mode"))
        {
            optimizer.debugMode = true;
            EditorUtility.SetDirty(optimizer);
        }
        
        if (GUILayout.Button("Test Light Switch"))
        {
            TestLightSwitch();
        }
    }

    private void TestLightSwitch()
    {
        if (optimizer == null || optimizer.rooms == null || optimizer.rooms.Count == 0) 
        {
            Debug.LogError("No rooms to test!");
            return;
        }
        
        // Выбираем первую комнату для теста
        var testRoom = optimizer.rooms[0];
        if (testRoom == null || testRoom.roomLights.Count == 0)
        {
            Debug.LogError("No lights in the first room to test!");
            return;
        }
        
        // Проверяем текущее состояние света
        bool allOn = true;
        foreach (var light in testRoom.roomLights)
        {
            if (light != null && !light.enabled)
            {
                allOn = false;
                break;
            }
        }
        
        // Переключаем все источники света на противоположное состояние
        foreach (var light in testRoom.roomLights)
        {
            if (light != null)
            {
                light.enabled = !allOn;
            }
        }
        
        Debug.Log($"Test: Switching all lights in room '{testRoom.roomName}' to {(!allOn ? "ON" : "OFF")}");
    }

    // Находит индекс комнаты в списке rooms по SerializedProperty
    private int GetRoomIndex(SerializedProperty roomProperty)
    {
        if (roomProperty == null || roomProperty.objectReferenceValue == null) 
            return -1;
            
        string roomPath = roomProperty.propertyPath;
        
        // Пытаемся извлечь индекс из пути свойства
        int startIndex = roomPath.LastIndexOf("[");
        int endIndex = roomPath.LastIndexOf("]");
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            string indexStr = roomPath.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (int.TryParse(indexStr, out int index))
            {
                return index;
            }
        }
        
        // Альтернативный метод - ищем по имени
        for (int i = 0; i < optimizer.rooms.Count; i++)
        {
            // Проверяем по ID объекта
            if (optimizer.rooms[i].GetHashCode() == roomProperty.objectReferenceValue.GetHashCode())
            {
                return i;
            }
        }
        
        return -1;
    }
    
    private GameObject CreatePortalObject(string name, Color color)
    {
        // Создаем новый объект для портала
        GameObject portalObj = new GameObject(name);
        portalObj.transform.SetParent(optimizer.transform);
        
        // Добавляем визуальное представление для удобства
        GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualObj.name = "PortalVisual";
        visualObj.transform.SetParent(portalObj.transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = new Vector3(0.5f, 2f, 0.1f);
        
        // Удаляем коллайдер с визуального представления
        DestroyImmediate(visualObj.GetComponent<Collider>());
        
        // Настраиваем материал
        Renderer renderer = visualObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Создаем полупрозрачный материал
            color.a = 0.5f;
            Material portalMaterial = new Material(Shader.Find("Standard"));
            portalMaterial.color = color;
            renderer.sharedMaterial = portalMaterial;
            
            // Делаем материал полупрозрачным
            portalMaterial.SetFloat("_Mode", 3);
            portalMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            portalMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            portalMaterial.SetInt("_ZWrite", 0);
            portalMaterial.DisableKeyword("_ALPHATEST_ON");
            portalMaterial.EnableKeyword("_ALPHABLEND_ON");
            portalMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            portalMaterial.renderQueue = 3000;
        }
        
        return portalObj;
    }
    
    private void AutoGenerateConnections()
    {
        // Проверяем достаточное количество комнат
        if (optimizer.rooms.Count < 2)
        {
            EditorUtility.DisplayDialog("Error", "Нужно минимум 2 комнаты для создания соединений", "OK");
            return;
        }
        
        // Очищаем существующие соединения
        if (EditorUtility.DisplayDialog("Warning", 
            "Эта операция удалит все существующие соединения. Продолжить?", 
            "Да", "Отмена"))
        {
            optimizer.portalConnections.Clear();
            
            // Создаем соединения между соседними комнатами
            for (int i = 0; i < optimizer.rooms.Count - 1; i++)
            {
                // Создаем соединение между текущей и следующей комнатой
                CreateConnectionBetweenRooms(optimizer.rooms[i], optimizer.rooms[i+1]);
            }
            
            // Соединяем последнюю комнату с первой для образования "кольца" при желании
            if (optimizer.rooms.Count > 2 && 
                EditorUtility.DisplayDialog("Create Ring", 
                "Создать соединение между последней и первой комнатой?", 
                "Да", "Нет"))
            {
                CreateConnectionBetweenRooms(optimizer.rooms[optimizer.rooms.Count-1], optimizer.rooms[0]);
            }
            
            Debug.Log($"[RoomOptimizer] Created {optimizer.portalConnections.Count} portal connections");
        }
    }
    
    private void CreateConnectionBetweenRooms(RoomOptimizer.Room sourceRoom, RoomOptimizer.Room destRoom)
    {
        if (sourceRoom == null || destRoom == null || 
            sourceRoom.roomCenter == null || destRoom.roomCenter == null)
        {
            return;
        }
        
        string connectionName = $"{sourceRoom.roomName}_to_{destRoom.roomName}";
        
        // Создаем порталы
        GameObject prepPortalObj = CreatePortalObject($"PrepPortal_{connectionName}", Color.green);
        GameObject transPortalObj = CreatePortalObject($"TransPortal_{connectionName}", Color.red);
        
        // Размещаем порталы между комнатами
        Vector3 direction = (destRoom.roomCenter.position - sourceRoom.roomCenter.position).normalized;
        float distance = Vector3.Distance(sourceRoom.roomCenter.position, destRoom.roomCenter.position);
        
        // Размещаем порталы
        prepPortalObj.transform.position = sourceRoom.roomCenter.position + direction * (distance * 0.33f);
        transPortalObj.transform.position = sourceRoom.roomCenter.position + direction * (distance * 0.66f);
        
        // Разворачиваем порталы перпендикулярно направлению
        prepPortalObj.transform.forward = direction;
        transPortalObj.transform.forward = direction;
            
        // Создаем соединение
        RoomOptimizer.PortalConnection connection = new RoomOptimizer.PortalConnection
        {
            connectionName = connectionName,
            sourceRoom = sourceRoom,
            destinationRoom = destRoom,
            preparationPortal = prepPortalObj.transform,
            transitionPortal = transPortalObj.transform,
            useColliders = true
        };
        
        optimizer.portalConnections.Add(connection);
        optimizer.SetupPortalColliders(connection);
        
        // Создаем обратное соединение
        string reverseConnectionName = $"{destRoom.roomName}_to_{sourceRoom.roomName}";
        
        GameObject reversePrepPortal = CreatePortalObject($"PrepPortal_{reverseConnectionName}", Color.cyan);
        GameObject reverseTransPortal = CreatePortalObject($"TransPortal_{reverseConnectionName}", Color.magenta);
        
        reversePrepPortal.transform.position = destRoom.roomCenter.position - direction * (distance * 0.33f);
        reverseTransPortal.transform.position = destRoom.roomCenter.position - direction * (distance * 0.66f);
        
        reversePrepPortal.transform.forward = -direction;
        reverseTransPortal.transform.forward = -direction;
        
        RoomOptimizer.PortalConnection reverseConnection = new RoomOptimizer.PortalConnection
        {
            connectionName = reverseConnectionName,
            sourceRoom = destRoom,
            destinationRoom = sourceRoom,
            preparationPortal = reversePrepPortal.transform,
            transitionPortal = reverseTransPortal.transform,
            useColliders = true
        };
        
        optimizer.portalConnections.Add(reverseConnection);
        optimizer.SetupPortalColliders(reverseConnection);
    }

    // Заменяем метод OnSceneGUI для правильной работы с перемещением в сцене
    public void OnSceneGUI()
    {
        if (optimizer == null) return;
        
        // Для каждой комнаты добавляем возможность перемещения в сцене
        for (int i = 0; i < optimizer.rooms.Count; i++)
        {
            var room = optimizer.rooms[i];
            if (room == null || room.roomCenter == null) continue;
            
            // Если эта комната выбрана в иерархии, показываем ручки для перемещения
            if (Selection.activeGameObject == room.roomCenter.gameObject)
            {
                // Рисуем ручки перемещения
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(room.roomCenter.position, Quaternion.identity);
                
                if (EditorGUI.EndChangeCheck())
                {
                    // Регистрируем операцию для возможности отмены
                    Undo.RecordObject(room.roomCenter, "Move Room Center");
                    
                    // Проверяем, нужно ли перемещать всю комнату или только центр
                    bool moveWithContents = Event.current.shift;
                    
                    if (moveWithContents)
                    {
                        // Перемещаем комнату вместе с содержимым
                        Undo.RecordObject(optimizer, "Move Room with Contents");
                        optimizer.MoveRoom(room, newPosition);
                    }
                    else
                    {
                        // Перемещаем только центр комнаты
                        room.roomCenter.position = newPosition;
                    }
                    
                    EditorUtility.SetDirty(optimizer);
                }
                
                // Добавляем подсказку в сцене
                Handles.BeginGUI();
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;
                style.normal.background = EditorGUIUtility.whiteTexture;
                style.padding = new RectOffset(5, 5, 5, 5);
                
                Vector2 screenPos = HandleUtility.WorldToGUIPoint(room.roomCenter.position + Vector3.up * 2);
                Rect rect = new Rect(screenPos.x - 100, screenPos.y - 50, 200, 40);
                
                GUI.color = new Color(0, 0, 0, 0.8f);
                GUI.Box(rect, "");
                GUI.color = Color.white;
                
                GUI.Label(rect, "Shift + перемещение = \nпереместить с содержимым", style);
                Handles.EndGUI();
            }
        }
    }
}
#endif 