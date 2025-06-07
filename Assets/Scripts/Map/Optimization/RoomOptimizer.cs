using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomOptimizer : MonoBehaviour
{
    [Header("General Settings")]
    public Transform player;               // Ссылка на трансформ игрока
    public float portalTriggerRadius = 2f; // Радиус действия портала
    public bool debugMode = false;         // Режим отладки
    public Room initialActiveRoom;         // Начальная активная комната

    // Классы и свойства для комнат
    [System.Serializable]
    public class Room
    {
        public string roomName;          // Имя комнаты для удобства
        public Transform roomCenter;     // Центральная точка комнаты (опционально)
        public Transform roomBounds;     // Границы комнаты (объект с коллайдером)
        public List<Light> roomLights;   // Источники света в комнате

        public Room()
        {
            roomName = "New Room";
            roomLights = new List<Light>();
        }
        
        // Проверяет, находится ли точка внутри комнаты
        public bool IsPointInside(Vector3 point)
        {
            if (roomBounds == null) return false;
            
            Collider collider = roomBounds.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.Contains(point);
            }
            
            // Если нет коллайдера, но есть центр, используем примерные размеры
            if (roomCenter != null)
            {
                Vector3 roomSize = roomBounds != null ? roomBounds.localScale : new Vector3(10f, 3f, 10f);
                Bounds bounds = new Bounds(roomCenter.position, roomSize);
                return bounds.Contains(point);
            }
            
            return false;
        }
    }

    // Класс для описания соединения между комнатами через порталы
    [System.Serializable]
    public class PortalConnection
    {
        public string connectionName;        // Имя соединения
        public Room sourceRoom;              // Исходная комната
        public Room destinationRoom;         // Целевая комната
        public Transform preparationPortal;  // Первый промежуточный портал (подготовка)
        public Transform transitionPortal;   // Второй портал (выключение света)
        
        [Range(0f, 5f)]
        public float transitionDelay = 0.5f; // Задержка между активацией порталов
        
        // Для альтернативного варианта с коллайдерами
        public bool useColliders = false;    // Использовать ли коллайдеры вместо расчета расстояний
        public bool preparationTriggered = false; // Состояние первого портала
        public bool transitionTriggered = false;  // Состояние второго портала
        
        // Добавляем переменную для отслеживания предыдущего состояния
        [HideInInspector] public bool wasPreparationTriggered = false;
        [HideInInspector] public bool wasTransitionTriggered = false;
    }

    [Header("Room Settings")]
    [SerializeField] public List<Room> rooms = new List<Room>();

    [Header("Portal Settings")]
    [SerializeField] public List<PortalConnection> portalConnections = new List<PortalConnection>();

    // Состояние активных комнат
    private Room currentActiveRoom;
    private List<Room> activeRooms = new List<Room>();
    private Dictionary<Room, bool> roomStates = new Dictionary<Room, bool>();
    
    private void Start()
    {
        // Проверка наличия игрока
        if (player == null)
        {
            // Пробуем найти игрока по тегу
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
            }
            else
            {
                Debug.LogError("[RoomOptimizer] Player not found! The system will not function correctly.");
            }
        }
        
        // Создаем начальный словарь состояний комнат (все неактивны)
        roomStates = new Dictionary<Room, bool>();
        
        // Инициализируем список активных комнат
        activeRooms = new List<Room>();
        
        // Инициализация всех комнат в выключенном состоянии
        foreach (Room room in rooms)
        {
            if (room == null) continue;
            
            if (debugMode)
                Debug.Log($"[RoomOptimizer] Initializing room: {room.roomName} with {room.roomLights.Count} lights");
            
            // Проверяем свет в комнате
            if (room.roomLights.Count == 0)
            {
                if (debugMode)
                    Debug.LogWarning($"[RoomOptimizer] Room {room.roomName} has no lights!");
            }
            
            // Изначально все комнаты неактивны
            roomStates[room] = false;
            
            // Выключаем весь свет
            foreach (Light light in room.roomLights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
                else if (debugMode)
                {
                    Debug.LogWarning($"[RoomOptimizer] Null light found in room {room.roomName}");
                }
            }
        }
        
        // Активируем начальную комнату, если она указана
        if (initialActiveRoom != null)
        {
            if (debugMode)
                Debug.Log($"[RoomOptimizer] Setting initial active room: {initialActiveRoom.roomName}");
            
            ActivateRoom(initialActiveRoom);
            currentActiveRoom = initialActiveRoom;
        }
        else if (rooms.Count > 0)
        {
            // Если начальная комната не указана, активируем первую в списке
            if (debugMode)
                Debug.Log($"[RoomOptimizer] No initial room specified. Activating first room: {rooms[0].roomName}");
            
            ActivateRoom(rooms[0]);
            currentActiveRoom = rooms[0];
        }
        
        // Проверка порталов и соединений
        if (portalConnections.Count == 0)
        {
            if (debugMode)
                Debug.LogWarning("[RoomOptimizer] No portal connections defined!");
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"[RoomOptimizer] Portal connections found: {portalConnections.Count}");
                foreach (var connection in portalConnections)
                {
                    if (connection.sourceRoom == null || connection.destinationRoom == null)
                    {
                        Debug.LogError($"[RoomOptimizer] Connection {connection.connectionName} has missing rooms!");
                    }
                    
                    if (connection.preparationPortal == null || connection.transitionPortal == null)
                    {
                        Debug.LogError($"[RoomOptimizer] Connection {connection.connectionName} has missing portals!");
                    }
                }
            }
        }
        
        // Проверка радиуса срабатывания
        if (portalTriggerRadius <= 0)
        {
            Debug.LogError("[RoomOptimizer] Portal trigger radius must be greater than 0! Setting to default value of 2.0");
            portalTriggerRadius = 2.0f;
        }
    }
    
    private void Update()
    {
        // Периодически проверяем, в какой комнате находится игрок
        if (Time.frameCount % 30 == 0) // Примерно каждые 0.5 секунд при 60 FPS
        {
            UpdateCurrentRoom();
        }
        
        // Проверяем активные порталы
        CheckPortals();
    }
    
    private void CheckPortals()
    {
        if (player == null)
        {
            if (debugMode) Debug.LogWarning("[RoomOptimizer] Player reference is missing!");
            return;
        }

        // Удаляем проблемный код, использующий старый Input API
        // Debugging info can be shown without requiring key presses
        if (debugMode)
        {
            // Log debug info occasionally rather than on key press
            if (Time.frameCount % 300 == 0)  // Log every ~5 seconds at 60fps
            {
                Debug.Log($"[RoomOptimizer] Player position: {player.position}, In room: {(currentActiveRoom != null ? currentActiveRoom.roomName : "None")}");
                Debug.Log($"[RoomOptimizer] Active rooms: {activeRooms.Count}, Total rooms: {rooms.Count}");
            }
        }

        foreach (var connection in portalConnections)
        {
            // Проверяем только корректно настроенные соединения
            if (connection == null || 
                connection.sourceRoom == null || 
                connection.destinationRoom == null || 
                connection.preparationPortal == null || 
                connection.transitionPortal == null)
            {
                if (debugMode) Debug.LogWarning("[RoomOptimizer] Incomplete connection found! Check Source/Destination rooms and portals.");
                continue;
            }
                
            // Сохраняем предыдущее состояние триггеров
            bool prevPrepTriggered = connection.preparationTriggered;
            bool prevTransTriggered = connection.transitionTriggered;
                
            // Проверяем первый (подготовительный) портал
            float distanceToPreparationPortal = Vector3.Distance(player.position, connection.preparationPortal.position);
            bool isNearPrepPortal = distanceToPreparationPortal <= portalTriggerRadius;
            connection.preparationTriggered = isNearPrepPortal;
            
            // Проверяем второй (переходной) портал
            float distanceToTransitionPortal = Vector3.Distance(player.position, connection.transitionPortal.position);
            bool isNearTransPortal = distanceToTransitionPortal <= portalTriggerRadius;
            connection.transitionTriggered = isNearTransPortal;
            
            // Логика активации/деактивации комнат
            // 1. Если срабатывает первый портал - активировать конечную комнату
            if (connection.preparationTriggered && !connection.wasPreparationTriggered)
            {
                if (!roomStates.ContainsKey(connection.destinationRoom) || !roomStates[connection.destinationRoom])
                {
                    ActivateRoom(connection.destinationRoom);
                    if (debugMode)
                        Debug.Log($"[RoomOptimizer] PREP PORTAL TRIGGERED: {connection.connectionName}. " +
                                $"Activating destination room: {connection.destinationRoom.roomName}");
                }
            }
            
            // 2. Если срабатывает второй портал - деактивировать исходную комнату
            if (connection.transitionTriggered && !connection.wasTransitionTriggered)
            {
                if (roomStates.ContainsKey(connection.sourceRoom) && roomStates[connection.sourceRoom])
                {
                    // Добавляем явную отладочную информацию перед выключением света
                    if (debugMode)
                    {
                        Debug.Log($"[RoomOptimizer] TRANS PORTAL TRIGGERED: {connection.connectionName}");
                        Debug.Log($"[RoomOptimizer] Deactivating source room: {connection.sourceRoom.roomName}");
                        Debug.Log($"[RoomOptimizer] Source room has {connection.sourceRoom.roomLights.Count} lights");
                    }
                    
                    // Выключаем свет в исходной комнате
                    DeactivateRoom(connection.sourceRoom);
                    
                    // Проверяем, действительно ли свет выключился
                    if (debugMode)
                    {
                        bool allLightsOff = true;
                        foreach (var light in connection.sourceRoom.roomLights)
                        {
                            if (light != null && light.enabled)
                            {
                                allLightsOff = false;
                                Debug.LogError($"[RoomOptimizer] Light {light.name} is still enabled after deactivation!");
                            }
                        }
                        
                        if (allLightsOff)
                        {
                            Debug.Log($"[RoomOptimizer] All lights in room {connection.sourceRoom.roomName} are now OFF");
                        }
                    }
                    
                    // Обновляем текущую активную комнату только при прохождении второго портала
                    currentActiveRoom = connection.destinationRoom;
                }
            }
            
            // Обновляем состояния для следующей итерации
            connection.wasPreparationTriggered = connection.preparationTriggered;
            connection.wasTransitionTriggered = connection.transitionTriggered;
        }
        
        // Добавляем периодический вывод состояния всех комнат
        if (debugMode && Time.frameCount % 300 == 0) // Примерно каждые 5 секунд при 60 FPS
        {
            Debug.Log("[RoomOptimizer] Room States:");
            foreach (var kvp in roomStates)
            {
                if (kvp.Key != null)
                    Debug.Log($" - Room: {kvp.Key.roomName}, Active: {kvp.Value}, Lights: {kvp.Key.roomLights.Count}");
            }
        }
    }
    
    private void UpdateCurrentRoom()
    {
        // Находим комнату, в которой сейчас находится игрок
        foreach (var room in rooms)
        {
            if (room.IsPointInside(player.position))
            {
                ActivateRoom(room);
                currentActiveRoom = room;
                if (debugMode) Debug.Log($"[RoomOptimizer] Player is in room: {room.roomName}");
                return;
            }
        }
        
        if (debugMode) Debug.Log("[RoomOptimizer] Player is not in any defined room");
    }
    
    private void ActivateRoom(Room room)
    {
        if (room == null)
        {
            if (debugMode) Debug.LogError("[RoomOptimizer] Attempting to activate NULL room!");
            return;
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Activating room: {room.roomName}");
        
        // Включаем свет в комнате
        int lightsEnabled = 0;
        foreach (Light light in room.roomLights)
        {
            if (light != null)
            {
                light.enabled = true;
                lightsEnabled++;
            }
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Enabled {lightsEnabled}/{room.roomLights.Count} lights in room {room.roomName}");
        
        // Добавляем комнату в список активных
        if (!activeRooms.Contains(room))
        {
            activeRooms.Add(room);
        }
        
        // Обновляем состояние комнаты
        roomStates[room] = true;
    }
    
    private void DeactivateRoom(Room room)
    {
        if (room == null)
        {
            if (debugMode) Debug.LogError("[RoomOptimizer] Attempting to deactivate NULL room!");
            return;
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Deactivating room: {room.roomName}");
        
        // Выключаем свет в комнате
        int lightsDisabled = 0;
        foreach (Light light in room.roomLights)
        {
            if (light != null)
            {
                light.enabled = false;
                lightsDisabled++;
            }
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Disabled {lightsDisabled}/{room.roomLights.Count} lights in room {room.roomName}");
        
        // Удаляем комнату из списка активных
        activeRooms.Remove(room);
        
        // Обновляем состояние комнаты
        roomStates[room] = false;
    }
    
    // Публичный метод для добавления комнаты
    public void AddRoom(string name, Transform center, Vector3 size)
    {
        Room newRoom = new Room
        {
            roomName = name,
            roomCenter = center,
            roomBounds = new GameObject($"{name} Boundaries").transform
        };
        
        newRoom.roomBounds.localScale = size;
        newRoom.roomBounds.position = center.position;
        
        rooms.Add(newRoom);
        roomStates[newRoom] = false;
    }
    
    // Публичный метод для добавления соединения между комнатами
    public void AddPortalConnection(Room source, Room destination, Transform prepPortal, Transform transPortal)
    {
        PortalConnection connection = new PortalConnection
        {
            connectionName = $"{source.roomName} to {destination.roomName}",
            sourceRoom = source,
            destinationRoom = destination,
            preparationPortal = prepPortal,
            transitionPortal = transPortal,
            transitionDelay = 0.5f
        };
        
        portalConnections.Add(connection);
    }
    
    // Метод для автоматического поиска источников света в комнате
    public void DetectRoomLights(Room room)
    {
        if (room == null || room.roomCenter == null) return;
        
        room.roomLights.Clear();
        Bounds roomBounds = new Bounds(room.roomCenter.position, room.roomBounds.localScale);
        
        // Находим все источники света в сцене
        foreach (Light light in FindObjectsOfType<Light>())
        {
            if (light.type != LightType.Directional && roomBounds.Contains(light.transform.position))
            {
                room.roomLights.Add(light);
            }
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Detected {room.roomLights.Count} lights in room {room.roomName}");
    }
    
    // Добавляем публичный метод для перемещения комнаты
    public void MoveRoom(Room room, Vector3 newPosition)
    {
        if (room == null || room.roomCenter == null) return;
        
        // Вычисляем вектор смещения
        Vector3 offset = newPosition - room.roomCenter.position;
        
        // Перемещаем центр комнаты
        room.roomCenter.position = newPosition;
        
        // При необходимости можно также перемещать все объекты, связанные с комнатой
        // Например, все источники света
        if (room.roomLights != null)
        {
            foreach (Light light in room.roomLights)
            {
                if (light != null)
                {
                    light.transform.position += offset;
                }
            }
        }
        
        // И стены/препятствия
        if (room.roomBounds != null)
        {
            room.roomBounds.position += offset;
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Moved room: {room.roomName} to position: {newPosition}");
    }
    
    // Улучшаем метод OnDrawGizmos для более интерактивной визуализации
    private void OnDrawGizmos()
    {
        // Отображаем радиус триггера для порталов
        if (portalConnections == null) return;
        
        foreach (var connection in portalConnections)
        {
            if (connection.preparationPortal != null)
            {
                // Подготовительный портал - голубым цветом
                Gizmos.color = new Color(0, 0.8f, 1f, 0.3f); // полупрозрачный голубой
                Gizmos.DrawSphere(connection.preparationPortal.position, portalTriggerRadius);
                
                // Линия соединения между порталами
                if (connection.transitionPortal != null)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.8f); // синий
                    Gizmos.DrawLine(connection.preparationPortal.position, connection.transitionPortal.position);
                }
                
                // Отображаем текст с названием портала, если выбрана соответствующая опция
                #if UNITY_EDITOR
                if (debugMode)
                {
                    UnityEditor.Handles.color = Color.cyan;
                    UnityEditor.Handles.Label(connection.preparationPortal.position + Vector3.up * 0.5f, 
                        $"Prep: {connection.connectionName}");
                }
                #endif
            }
            
            if (connection.transitionPortal != null)
            {
                // Переходной портал - пурпурным цветом
                Gizmos.color = new Color(1f, 0, 1f, 0.3f); // полупрозрачный пурпурный
                Gizmos.DrawSphere(connection.transitionPortal.position, portalTriggerRadius);
                
                #if UNITY_EDITOR
                if (debugMode)
                {
                    UnityEditor.Handles.color = Color.magenta;
                    UnityEditor.Handles.Label(connection.transitionPortal.position + Vector3.up * 0.5f, 
                        $"Trans: {connection.connectionName}");
                }
                #endif
            }
            
            // Дополнительная информация о комнатах
            if (debugMode && connection.sourceRoom != null && connection.destinationRoom != null)
            {
                #if UNITY_EDITOR
                // Рисуем соединительные линии между комнатами
                if (connection.sourceRoom.roomCenter != null && connection.destinationRoom.roomCenter != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(connection.sourceRoom.roomCenter.position, 
                                 connection.destinationRoom.roomCenter.position);
                }
                #endif
            }
        }
        
        // Отображаем границы комнат
        foreach (var room in rooms)
        {
            if (room == null) continue;
            
            if (room.roomBounds != null)
            {
                Collider collider = room.roomBounds.GetComponent<Collider>();
                if (collider != null)
                {
                    // Используем разные цвета для активных и неактивных комнат
                    bool isActive = roomStates != null && roomStates.ContainsKey(room) && roomStates[room];
                    
                    if (isActive)
                    {
                        Gizmos.color = new Color(0, 1f, 0, 0.1f); // полупрозрачный зеленый
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 0, 0, 0.1f); // полупрозрачный красный
                    }
                    
                    // Отображаем границы в виде куба
                    Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
                    
                    // Отображаем проволочную рамку
                    Gizmos.color = isActive ? Color.green : Color.red;
                    Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
                    
                    #if UNITY_EDITOR
                    if (debugMode)
                    {
                        // Отображаем имя комнаты
                        UnityEditor.Handles.color = isActive ? Color.green : Color.red;
                        UnityEditor.Handles.Label(collider.bounds.center + Vector3.up * (collider.bounds.extents.y + 0.5f), 
                            $"{room.roomName} - {(isActive ? "ON" : "OFF")}");
                    }
                    #endif
                }
                else if (room.roomCenter != null)
                {
                    // Если нет коллайдера, но есть центр комнаты, рисуем простой маркер
                    bool isActive = roomStates != null && roomStates.ContainsKey(room) && roomStates[room];
                    Gizmos.color = isActive ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(room.roomCenter.position, 1f);
                    
                    #if UNITY_EDITOR
                    if (debugMode)
                    {
                        // Отображаем имя комнаты
                        UnityEditor.Handles.color = isActive ? Color.green : Color.red;
                        UnityEditor.Handles.Label(room.roomCenter.position + Vector3.up * 1.5f, 
                            $"{room.roomName} - {(isActive ? "ON" : "OFF")}");
                    }
                    #endif
                }
            }
            else if (room.roomCenter != null)
            {
                // Если нет границ, но есть центр комнаты, рисуем простой маркер
                bool isActive = roomStates != null && roomStates.ContainsKey(room) && roomStates[room];
                Gizmos.color = isActive ? Color.green : Color.red;
                Gizmos.DrawWireSphere(room.roomCenter.position, 1f);
                
                #if UNITY_EDITOR
                if (debugMode)
                {
                    // Отображаем имя комнаты
                    UnityEditor.Handles.color = isActive ? Color.green : Color.red;
                    UnityEditor.Handles.Label(room.roomCenter.position + Vector3.up * 1.5f, 
                        $"{room.roomName} - {(isActive ? "ON" : "OFF")}");
                }
                #endif
            }
        }
    }

    // Добавляем после метода CheckPortals()
    // Альтернативный способ с триггерами вместо дистанций
    public void OnTriggerEnter(Collider other)
    {
        // Проверяем, что коллайдер не null
        if (other == null) return;
        
        // Обрабатываем вход в триггер портала
        if (other.CompareTag("Player"))
        {
            // Ищем все порталы с коллайдерами
            foreach (var connection in portalConnections)
            {
                if (connection == null || !connection.useColliders) continue;
                
                // Проверяем, является ли коллайдер подготовительным порталом
                if (connection.preparationPortal != null && 
                    connection.preparationPortal.GetComponent<Collider>() == other)
                {
                    if (debugMode)
                        Debug.Log($"[RoomOptimizer] Player entered PREP portal trigger: {connection.connectionName}");
                    
                    connection.preparationTriggered = true;
                    
                    // Активируем целевую комнату
                    if (connection.destinationRoom != null && 
                        (!roomStates.ContainsKey(connection.destinationRoom) || !roomStates[connection.destinationRoom]))
                    {
                        ActivateRoom(connection.destinationRoom);
                        if (debugMode)
                            Debug.Log($"[RoomOptimizer] Activated destination room via trigger: {connection.destinationRoom.roomName}");
                    }
                }
                
                // Проверяем, является ли коллайдер переходным порталом
                if (connection.transitionPortal != null && 
                    connection.transitionPortal.GetComponent<Collider>() == other)
                {
                    if (debugMode)
                        Debug.Log($"[RoomOptimizer] Player entered TRANS portal trigger: {connection.connectionName}");
                    
                    connection.transitionTriggered = true;
                    
                    // Деактивируем исходную комнату
                    if (connection.sourceRoom != null && roomStates.ContainsKey(connection.sourceRoom) && roomStates[connection.sourceRoom])
                    {
                        DeactivateRoom(connection.sourceRoom);
                        if (debugMode)
                            Debug.Log($"[RoomOptimizer] Deactivated source room via trigger: {connection.sourceRoom.roomName}");
                            
                        // Обновляем текущую активную комнату
                        currentActiveRoom = connection.destinationRoom;
                    }
                }
            }
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        // Проверяем, что коллайдер не null
        if (other == null) return;
        
        // Обрабатываем выход из триггера портала
        if (other.CompareTag("Player"))
        {
            // Ищем все порталы с коллайдерами
            foreach (var connection in portalConnections)
            {
                if (connection == null || !connection.useColliders) continue;
                
                // Сбрасываем флаги порталов при выходе из триггеров
                if (connection.preparationPortal != null && 
                    connection.preparationPortal.GetComponent<Collider>() == other)
                {
                    if (debugMode)
                        Debug.Log($"[RoomOptimizer] Player exited PREP portal trigger: {connection.connectionName}");
                    
                    connection.preparationTriggered = false;
                }
                
                if (connection.transitionPortal != null && 
                    connection.transitionPortal.GetComponent<Collider>() == other)
                {
                    if (debugMode)
                        Debug.Log($"[RoomOptimizer] Player exited TRANS portal trigger: {connection.connectionName}");
                    
                    connection.transitionTriggered = false;
                }
            }
        }
    }

    private void OnGUI()
    {
        // Добавляем экранную подсказку в режиме отладки
        if (!Application.isPlaying || !debugMode) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        
        // Создаем фон для текста
        Texture2D backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.5f)); // полупрозрачный черный фон
        backgroundTexture.Apply();
        style.normal.background = backgroundTexture;
        
        // Отображаем информацию о системе на экране
        GUI.Label(new Rect(10, 10, 400, 300), 
            $"RoomOptimizer Debug:\n" +
            $"Player: {(player != null ? player.name : "NULL")}\n" +
            $"Position: {(player != null ? player.position.ToString("F1") : "NULL")}\n" +
            $"Active Room: {(currentActiveRoom != null ? currentActiveRoom.roomName : "NULL")}\n" +
            $"Active Rooms: {(activeRooms != null ? string.Join(", ", activeRooms.Select(r => r.roomName)) : "None")}\n" +
            $"Total Rooms: {(rooms != null ? rooms.Count.ToString() : "0")}\n" +
            $"Portal Connections: {portalConnections.Count}\n" +
            $"Active Portals: {portalConnections.Count(c => c.preparationTriggered || c.transitionTriggered)}\n" +
            $"\nПорталы рядом с игроком:\n" + 
            (player != null ? string.Join("\n", portalConnections
                .Where(c => c.preparationPortal != null && c.transitionPortal != null && 
                          (Vector3.Distance(player.position, c.preparationPortal.position) < portalTriggerRadius * 1.5f || 
                           Vector3.Distance(player.position, c.transitionPortal.position) < portalTriggerRadius * 1.5f))
                .Select(c => $" - {c.connectionName}: " + 
                    (Vector3.Distance(player.position, c.preparationPortal.position) < portalTriggerRadius ? 
                        "Подготовка [АКТИВЕН]" : "Подготовка") + " / " + 
                    (Vector3.Distance(player.position, c.transitionPortal.position) < portalTriggerRadius ? 
                        "Переход [АКТИВЕН]" : "Переход"))) : "Нет игрока")
            ,
            style);
        
        // Уничтожаем текстуру, чтобы не было утечек памяти
        Destroy(backgroundTexture);
    }

    // Метод для вызова в редакторе Unity для добавления инструкций
    public void ShowInstructions()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.DisplayDialog(
            "RoomOptimizer Instructions",
            "1. Создайте комнаты в вашей сцене и добавьте их в список Rooms.\n\n" +
            "2. Для каждой комнаты задайте:\n" +
            "   - roomName: уникальное имя комнаты\n" +
            "   - roomBounds: объект с коллайдером, определяющим границы комнаты\n" +
            "   - roomCenter: центральный объект комнаты (опционально)\n" +
            "   - roomLights: список источников света в комнате\n\n" +
            "3. Создайте порталы между комнатами:\n" +
            "   - Добавьте Portal Connection\n" +
            "   - Укажите исходную и целевую комнаты\n" +
            "   - Добавьте два портала: preparationPortal и transitionPortal\n" +
            "   - preparationPortal активирует следующую комнату\n" +
            "   - transitionPortal выключает свет в предыдущей комнате\n\n" +
            "4. Если свет не выключается:\n" +
            "   - Проверьте, что порталы размещены правильно\n" +
            "   - Включите Debug Mode для отображения отладочной информации\n" +
            "   - Попробуйте использовать режим Use Collider Triggers\n" +
            "   - Проверьте, что источники света правильно добавлены в комнаты\n\n" +
            "5. Визуализация в редакторе:\n" +
            "   - Голубые сферы: подготовительные порталы\n" +
            "   - Пурпурные сферы: переходные порталы\n" +
            "   - Зеленые рамки: активные комнаты\n" +
            "   - Красные рамки: неактивные комнаты",
            "OK"
        );
        #endif
    }

    // Публичный метод для проверки и исправления света
    public void CheckAndFixLights()
    {
        #if UNITY_EDITOR
        if (rooms == null) return;
        
        int totalFixed = 0;
        
        foreach (var room in rooms)
        {
            if (room == null) continue;
            
            // Удаляем null-ссылки
            room.roomLights.RemoveAll(light => light == null);
            
            // Если есть границы комнаты, ищем в них свет
            if (room.roomBounds != null)
            {
                Collider roomCollider = room.roomBounds.GetComponent<Collider>();
                if (roomCollider != null)
                {
                    Bounds bounds = roomCollider.bounds;
                    
                    // Находим все источники света на сцене
                    Light[] allLights = GameObject.FindObjectsOfType<Light>();
                    foreach (Light light in allLights)
                    {
                        // Проверяем, находится ли свет в границах комнаты и еще не добавлен
                        if (bounds.Contains(light.transform.position) && !room.roomLights.Contains(light))
                        {
                            room.roomLights.Add(light);
                            totalFixed++;
                            Debug.Log($"Added light '{light.name}' to room '{room.roomName}'");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Room '{room.roomName}' has roomBounds but no Collider component attached.");
                }
            }
            else if (room.roomCenter != null)
            {
                // Если нет границ, но есть центр, используем примерную область
                Bounds approxBounds = new Bounds(room.roomCenter.position, new Vector3(10f, 3f, 10f));
                
                // Находим все источники света на сцене
                Light[] allLights = GameObject.FindObjectsOfType<Light>();
                foreach (Light light in allLights)
                {
                    // Проверяем, находится ли свет в примерных границах комнаты и еще не добавлен
                    if (approxBounds.Contains(light.transform.position) && !room.roomLights.Contains(light))
                    {
                        room.roomLights.Add(light);
                        totalFixed++;
                        Debug.Log($"Added light '{light.name}' to room '{room.roomName}' (using approximate bounds)");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Room '{room.roomName}' has no boundaries or center point. Cannot detect lights automatically.");
            }
        }
        
        // Выводим информацию о результатах проверки
        if (totalFixed > 0)
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "Проверка источников света",
                $"Добавлено {totalFixed} новых источников света в комнаты.",
                "ОК"
            );
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "Проверка источников света",
                "Новых источников света не найдено.",
                "ОК"
            );
        }
        #endif
    }

    // Публичный метод для настройки порталов с коллайдерами
    public void SetupPortalColliders(PortalConnection connection)
    {
        if (connection == null) return;
        
        SetupSinglePortalCollider(connection.preparationPortal, "PrepPortalTrigger");
        SetupSinglePortalCollider(connection.transitionPortal, "TransPortalTrigger");
        
        connection.useColliders = true;
        
        if (debugMode)
            Debug.Log($"[RoomOptimizer] Setup portal colliders for connection: {connection.connectionName}");
    }
    
    private void SetupSinglePortalCollider(Transform portal, string triggerName)
    {
        if (portal == null) return;
        
        // Проверяем наличие коллайдера
        Collider portalCollider = portal.GetComponent<Collider>();
        
        if (portalCollider == null)
        {
            // Создаем сферический коллайдер
            SphereCollider sphereCollider = portal.gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = portalTriggerRadius;
            sphereCollider.isTrigger = true;
            portalCollider = sphereCollider;
            
            if (debugMode)
                Debug.Log($"[RoomOptimizer] Created sphere collider for portal: {portal.name}");
        }
        else
        {
            // Убедимся, что коллайдер - триггер
            portalCollider.isTrigger = true;
        }
        
        // Переименовываем для удобства
        if (!portal.name.Contains(triggerName))
            portal.name = $"{portal.name}_{triggerName}";
            
        // Добавляем компонент Rigidbody, если его нет (нужен для работы триггеров)
        if (portal.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = portal.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            
            if (debugMode)
                Debug.Log($"[RoomOptimizer] Added kinematic Rigidbody to portal: {portal.name}");
        }
    }
} 