#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RoomOptimizer))]
public class RoomOptimizerEditor : Editor
{
    private RoomOptimizer optimizer;
    private SerializedProperty rooms;
    private SerializedProperty portalConnections;
    private SerializedProperty player;
    private SerializedProperty playerDetectionMask;
    private SerializedProperty portalTriggerRadius;
    private SerializedProperty debugMode;
    
    private void OnEnable()
    {
        optimizer = (RoomOptimizer)target;
        rooms = serializedObject.FindProperty("rooms");
        portalConnections = serializedObject.FindProperty("portalConnections");
        player = serializedObject.FindProperty("player");
        playerDetectionMask = serializedObject.FindProperty("playerDetectionMask");
        portalTriggerRadius = serializedObject.FindProperty("portalTriggerRadius");
        debugMode = serializedObject.FindProperty("debugMode");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Room Optimizer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This component manages the transition between rooms using a two-portal system.\n" +
                                "1. The Preparation Portal activates the destination room\n" +
                                "2. The Transition Portal deactivates the source room", MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(player);
        EditorGUILayout.PropertyField(playerDetectionMask);
        EditorGUILayout.PropertyField(portalTriggerRadius);
        EditorGUILayout.PropertyField(debugMode);
        
        // Комнаты
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rooms", EditorStyles.boldLabel);
        
        // Кнопка для добавления новой комнаты
        if (GUILayout.Button("Add Room"))
        {
            rooms.arraySize++;
            SerializedProperty newRoom = rooms.GetArrayElementAtIndex(rooms.arraySize - 1);
            newRoom.FindPropertyRelative("roomName").stringValue = "Room " + rooms.arraySize;
            newRoom.FindPropertyRelative("roomSize").vector3Value = new Vector3(10f, 3f, 10f);
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.Space();
        
        // Отображение и редактирование комнат
        for (int i = 0; i < rooms.arraySize; i++)
        {
            SerializedProperty room = rooms.GetArrayElementAtIndex(i);
            SerializedProperty roomName = room.FindPropertyRelative("roomName");
            SerializedProperty roomSize = room.FindPropertyRelative("roomSize");
            SerializedProperty roomLights = room.FindPropertyRelative("roomLights");
            SerializedProperty roomWalls = room.FindPropertyRelative("roomWalls");
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
            EditorGUILayout.PropertyField(roomSize);
            
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
                            // Устанавливаем Y-позицию в точку удара плюс половина высоты комнаты
                            float roomHeight = roomSize.vector3Value.y;
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
            EditorGUILayout.PropertyField(roomWalls);
            
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
        
        // Соединения порталов
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Room Connections", EditorStyles.boldLabel);
        
        // Кнопка для добавления нового соединения
        if (GUILayout.Button("Add Portal Connection"))
        {
            portalConnections.arraySize++;
            SerializedProperty newConnection = portalConnections.GetArrayElementAtIndex(portalConnections.arraySize - 1);
            newConnection.FindPropertyRelative("connectionName").stringValue = "Connection " + portalConnections.arraySize;
            newConnection.FindPropertyRelative("transitionDelay").floatValue = 0.5f;
            serializedObject.ApplyModifiedProperties();
        }
        
        // Кнопка для автоматического создания соединений между комнатами
        if (GUILayout.Button("Auto-Generate Connections"))
        {
            AutoGenerateConnections();
        }
        
        EditorGUILayout.Space();
        
        // Отображение и редактирование соединений комнат
        for (int i = 0; i < portalConnections.arraySize; i++)
        {
            SerializedProperty connection = portalConnections.GetArrayElementAtIndex(i);
            SerializedProperty connectionName = connection.FindPropertyRelative("connectionName");
            SerializedProperty sourceRoom = connection.FindPropertyRelative("sourceRoom");
            SerializedProperty destinationRoom = connection.FindPropertyRelative("destinationRoom");
            SerializedProperty preparationPortal = connection.FindPropertyRelative("preparationPortal");
            SerializedProperty transitionPortal = connection.FindPropertyRelative("transitionPortal");
            SerializedProperty transitionDelay = connection.FindPropertyRelative("transitionDelay");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Заголовок соединения с возможностью удаления
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(connectionName.stringValue, EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                portalConnections.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                break;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(connectionName);
            
            // Комнаты
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rooms:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sourceRoom, new GUIContent("Source Room"));
            EditorGUILayout.PropertyField(destinationRoom, new GUIContent("Destination Room"));
            
            // Порталы
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Portals:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(preparationPortal, new GUIContent("Preparation Portal"));
            EditorGUILayout.PropertyField(transitionPortal, new GUIContent("Transition Portal"));
            
            // Настройки
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(transitionDelay);
            
            // Кнопки быстрого создания порталов
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Preparation Portal"))
            {
                // Создаем новый объект для подготовительного портала
                GameObject portalObj = CreatePortalObject($"PrepPortal_{connectionName.stringValue}", Color.cyan);
                
                // Размещаем портал на пути между комнатами
                if (sourceRoom.objectReferenceValue != null && destinationRoom.objectReferenceValue != null)
                {
                    // Получаем комнаты напрямую из списка оптимизатора
                    int sourceIndex = GetRoomIndex(sourceRoom);
                    int destIndex = GetRoomIndex(destinationRoom);
                    
                    if (sourceIndex >= 0 && destIndex >= 0)
                    {
                        var source = optimizer.rooms[sourceIndex];
                        var destination = optimizer.rooms[destIndex];
                        
                        if (source.roomCenter != null && destination.roomCenter != null)
                        {
                            // Размещаем портал на 1/3 пути между комнатами
                            portalObj.transform.position = Vector3.Lerp(
                                source.roomCenter.position, 
                                destination.roomCenter.position, 
                                0.33f
                            );
                        }
                    }
                }
                
                preparationPortal.objectReferenceValue = portalObj.transform;
                serializedObject.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("Create Transition Portal"))
            {
                // Создаем новый объект для переходного портала
                GameObject portalObj = CreatePortalObject($"TransPortal_{connectionName.stringValue}", Color.magenta);
                
                // Размещаем портал на пути между комнатами
                if (sourceRoom.objectReferenceValue != null && destinationRoom.objectReferenceValue != null)
                {
                    // Получаем комнаты напрямую из списка оптимизатора
                    int sourceIndex = GetRoomIndex(sourceRoom);
                    int destIndex = GetRoomIndex(destinationRoom);
                    
                    if (sourceIndex >= 0 && destIndex >= 0)
                    {
                        var source = optimizer.rooms[sourceIndex];
                        var destination = optimizer.rooms[destIndex];
                        
                        if (source.roomCenter != null && destination.roomCenter != null)
                        {
                            // Размещаем портал на 2/3 пути между комнатами
                            portalObj.transform.position = Vector3.Lerp(
                                source.roomCenter.position, 
                                destination.roomCenter.position, 
                                0.66f
                            );
                        }
                    }
                }
                
                transitionPortal.objectReferenceValue = portalObj.transform;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
            
            // Кнопка для создания обоих порталов одновременно
            if (GUILayout.Button("Create Both Portals"))
            {
                if (sourceRoom.objectReferenceValue != null && destinationRoom.objectReferenceValue != null)
                {
                    // Получаем комнаты напрямую из списка оптимизатора
                    int sourceIndex = GetRoomIndex(sourceRoom);
                    int destIndex = GetRoomIndex(destinationRoom);
                    
                    if (sourceIndex >= 0 && destIndex >= 0)
                    {
                        var source = optimizer.rooms[sourceIndex];
                        var destination = optimizer.rooms[destIndex];
                        
                        if (source.roomCenter != null && destination.roomCenter != null)
                        {
                            // Создаем подготовительный портал
                            GameObject prepPortalObj = CreatePortalObject($"PrepPortal_{connectionName.stringValue}", Color.cyan);
                            prepPortalObj.transform.position = Vector3.Lerp(
                                source.roomCenter.position, 
                                destination.roomCenter.position, 
                                0.33f
                            );
                            preparationPortal.objectReferenceValue = prepPortalObj.transform;
                            
                            // Создаем переходной портал
                            GameObject transPortalObj = CreatePortalObject($"TransPortal_{connectionName.stringValue}", Color.magenta);
                            transPortalObj.transform.position = Vector3.Lerp(
                                source.roomCenter.position, 
                                destination.roomCenter.position, 
                                0.66f
                            );
                            transitionPortal.objectReferenceValue = transPortalObj.transform;
                            
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // Отображение подсказки для настройки игрока
        if (player.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Player reference is not set. The optimizer will try to find a GameObject with the 'Player' tag at runtime.", MessageType.Warning);
        }
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
        GameObject portalObj = new GameObject(name);
        portalObj.transform.SetParent(optimizer.transform);
        
        // Добавляем сферический коллайдер-триггер
        SphereCollider collider = portalObj.AddComponent<SphereCollider>();
        collider.radius = portalTriggerRadius.floatValue;
        collider.isTrigger = true;
        
        // Добавляем визуальный маркер
        GameObject visualMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualMarker.name = "VisualMarker";
        visualMarker.transform.SetParent(portalObj.transform);
        visualMarker.transform.localPosition = Vector3.zero;
        visualMarker.transform.localScale = Vector3.one * 0.3f;
        
        // Удаляем коллайдер с визуального маркера
        DestroyImmediate(visualMarker.GetComponent<Collider>());
        
        // Делаем маркер полупрозрачным
        Renderer renderer = visualMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(color.r, color.g, color.b, 0.5f);
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
        }
        
        return portalObj;
    }
    
    private void AutoGenerateConnections()
    {
        if (rooms.arraySize < 2)
        {
            EditorUtility.DisplayDialog("Error", "You need at least 2 rooms to generate connections", "OK");
            return;
        }
        
        // Находим все существующие пары комнат
        HashSet<string> existingPairs = new HashSet<string>();
        for (int i = 0; i < portalConnections.arraySize; i++)
        {
            SerializedProperty connection = portalConnections.GetArrayElementAtIndex(i);
            SerializedProperty sourceRoom = connection.FindPropertyRelative("sourceRoom");
            SerializedProperty destinationRoom = connection.FindPropertyRelative("destinationRoom");
            
            if (sourceRoom.objectReferenceValue != null && destinationRoom.objectReferenceValue != null)
            {
                int sourceIndex = GetRoomIndex(sourceRoom);
                int destIndex = GetRoomIndex(destinationRoom);
                
                if (sourceIndex >= 0 && destIndex >= 0)
                {
                    existingPairs.Add($"{sourceIndex}_{destIndex}");
                }
            }
        }
        
        // Создаем соединения для всех возможных пар комнат
        int connectionsAdded = 0;
        
        for (int i = 0; i < rooms.arraySize; i++)
        {
            for (int j = 0; j < rooms.arraySize; j++)
            {
                if (i == j) continue; // Пропускаем соединение комнаты с самой собой
                
                string pairKey = $"{i}_{j}";
                if (existingPairs.Contains(pairKey)) continue; // Пропускаем уже существующие соединения
                
                // Создаем новое соединение
                portalConnections.arraySize++;
                SerializedProperty newConnection = portalConnections.GetArrayElementAtIndex(portalConnections.arraySize - 1);
                
                // Получаем данные комнат
                SerializedProperty srcRoom = rooms.GetArrayElementAtIndex(i);
                SerializedProperty dstRoom = rooms.GetArrayElementAtIndex(j);
                SerializedProperty srcName = srcRoom.FindPropertyRelative("roomName");
                SerializedProperty dstName = dstRoom.FindPropertyRelative("roomName");
                
                newConnection.FindPropertyRelative("connectionName").stringValue = 
                    $"{srcName.stringValue} to {dstName.stringValue}";
                
                // Устанавливаем ссылки на комнаты (здесь используем индексы)
                newConnection.FindPropertyRelative("sourceRoom").objectReferenceInstanceIDValue = 
                    srcRoom.objectReferenceInstanceIDValue;
                    
                newConnection.FindPropertyRelative("destinationRoom").objectReferenceInstanceIDValue = 
                    dstRoom.objectReferenceInstanceIDValue;
                    
                newConnection.FindPropertyRelative("transitionDelay").floatValue = 0.5f;
                
                // Создаем порталы
                SerializedProperty srcCenter = srcRoom.FindPropertyRelative("roomCenter");
                SerializedProperty dstCenter = dstRoom.FindPropertyRelative("roomCenter");
                
                if (srcCenter.objectReferenceValue != null && dstCenter.objectReferenceValue != null)
                {
                    Transform sourceTransform = srcCenter.objectReferenceValue as Transform;
                    Transform destTransform = dstCenter.objectReferenceValue as Transform;
                    
                    // Создаем подготовительный портал
                    GameObject prepPortalObj = CreatePortalObject($"PrepPortal_{srcName.stringValue}_to_{dstName.stringValue}", Color.cyan);
                    prepPortalObj.transform.position = Vector3.Lerp(sourceTransform.position, destTransform.position, 0.33f);
                    newConnection.FindPropertyRelative("preparationPortal").objectReferenceValue = prepPortalObj.transform;
                    
                    // Создаем переходной портал
                    GameObject transPortalObj = CreatePortalObject($"TransPortal_{srcName.stringValue}_to_{dstName.stringValue}", Color.magenta);
                    transPortalObj.transform.position = Vector3.Lerp(sourceTransform.position, destTransform.position, 0.66f);
                    newConnection.FindPropertyRelative("transitionPortal").objectReferenceValue = transPortalObj.transform;
                }
                
                connectionsAdded++;
                existingPairs.Add(pairKey);
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Auto-Generate Connections", $"Added {connectionsAdded} new connections between rooms.", "OK");
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