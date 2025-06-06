using UnityEngine;
using System.Collections.Generic;

public class RoomOptimizer : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public string roomName;
        public Vector3 roomSize = new Vector3(10f, 3f, 10f);
        public List<Light> roomLights = new List<Light>();
        public List<GameObject> roomWalls = new List<GameObject>();
        public Transform roomCenter;

        // Проверяет, находится ли точка внутри комнаты
        public bool IsPointInside(Vector3 point)
        {
            if (roomCenter == null) return false;
            Bounds bounds = new Bounds(roomCenter.position, roomSize);
            return bounds.Contains(point);
        }
    }

    [System.Serializable]
    public class PortalConnection
    {
        public string connectionName;
        public Room sourceRoom;            // Исходная комната
        public Room destinationRoom;       // Целевая комната
        public Transform preparationPortal; // Первый промежуточный портал (подготовка)
        public Transform transitionPortal;  // Второй портал (выключение света)
        [Range(0f, 5f)]
        public float transitionDelay = 0.5f; // Задержка между активацией порталов
    }

    [Header("Room Settings")]
    [SerializeField] public List<Room> rooms = new List<Room>();

    [Header("Portal Settings")]
    [SerializeField] public List<PortalConnection> portalConnections = new List<PortalConnection>();
    [SerializeField] public Transform player;
    [SerializeField] public LayerMask playerDetectionMask;
    [SerializeField] public float portalTriggerRadius = 1.5f;
    [SerializeField] public bool debugMode = true;

    // Состояние активных комнат
    private Room currentActiveRoom;
    private List<Room> activeRooms = new List<Room>();
    private Dictionary<Room, bool> roomStates = new Dictionary<Room, bool>();
    
    private void Start()
    {
        // Если игрок не задан, ищем объект с тегом "Player"
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("[RoomOptimizer] Player not found! Please tag your player with 'Player' or assign it directly.");
                enabled = false;
                return;
            }
        }
        
        // Инициализируем состояние всех комнат
        foreach (var room in rooms)
        {
            if (room != null && !roomStates.ContainsKey(room))
            {
                roomStates[room] = false;
            }
        }

        foreach (var connection in portalConnections)
        {
            if (connection.sourceRoom != null && !roomStates.ContainsKey(connection.sourceRoom))
            {
                roomStates[connection.sourceRoom] = false;
            }
            
            if (connection.destinationRoom != null && !roomStates.ContainsKey(connection.destinationRoom))
            {
                roomStates[connection.destinationRoom] = false;
            }
        }
        
        // Находим начальную активную комнату (где находится игрок)
        UpdateCurrentRoom();
    }
    
    private void Update()
    {
        // Проверяем активные порталы
        CheckPortals();
    }
    
    private void CheckPortals()
    {
        foreach (var connection in portalConnections)
        {
            // Проверяем только корректно настроенные соединения
            if (connection.sourceRoom == null || connection.destinationRoom == null || 
                connection.preparationPortal == null || connection.transitionPortal == null)
                continue;
                
            // Проверяем первый (подготовительный) портал
            float distanceToPreparationPortal = Vector3.Distance(player.position, connection.preparationPortal.position);
            if (distanceToPreparationPortal <= portalTriggerRadius)
            {
                // Активируем подготовку целевой комнаты, но пока не выключаем исходную
                if (!roomStates.ContainsKey(connection.destinationRoom) || !roomStates[connection.destinationRoom])
                {
                    ActivateRoom(connection.destinationRoom);
                    if (debugMode) 
                        Debug.Log($"[RoomOptimizer] Preparation portal triggered for {connection.connectionName}. " +
                                  $"Activating destination room: {connection.destinationRoom.roomName}");
                }
            }
            
            // Проверяем второй (переходной) портал
            float distanceToTransitionPortal = Vector3.Distance(player.position, connection.transitionPortal.position);
            if (distanceToTransitionPortal <= portalTriggerRadius)
            {
                // Деактивируем исходную комнату
                if (roomStates.ContainsKey(connection.sourceRoom) && roomStates[connection.sourceRoom])
                {
                    DeactivateRoom(connection.sourceRoom);
                    if (debugMode) 
                        Debug.Log($"[RoomOptimizer] Transition portal triggered for {connection.connectionName}. " +
                                  $"Deactivating source room: {connection.sourceRoom.roomName}");
                }
                
                // Обновляем текущую активную комнату
                currentActiveRoom = connection.destinationRoom;
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
        if (room == null) return;
        
        // Включаем свет в комнате
        foreach (Light light in room.roomLights)
        {
            if (light != null) light.enabled = true;
        }
        
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
        if (room == null) return;
        
        // Выключаем свет в комнате
        foreach (Light light in room.roomLights)
        {
            if (light != null) light.enabled = false;
        }
        
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
            roomSize = size
        };
        
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
        Bounds roomBounds = new Bounds(room.roomCenter.position, room.roomSize);
        
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
        if (room.roomWalls != null)
        {
            foreach (GameObject wall in room.roomWalls)
            {
                if (wall != null)
                {
                    wall.transform.position += offset;
                }
            }
        }
        
        if (debugMode) Debug.Log($"[RoomOptimizer] Moved room: {room.roomName} to position: {newPosition}");
    }
    
    // Улучшаем метод OnDrawGizmos для более интерактивной визуализации
    private void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        // Визуализируем комнаты
        foreach (var room in rooms)
        {
            if (room == null || room.roomCenter == null) continue;
            
            // Определяем цвет комнаты в зависимости от её активности
            bool isActive = roomStates.ContainsKey(room) && roomStates[room];
            Color roomFillColor = isActive ? new Color(0.2f, 0.8f, 0.2f, 0.2f) : new Color(0.8f, 0.2f, 0.2f, 0.2f);
            Color roomWireColor = isActive ? new Color(0.2f, 0.8f, 0.2f, 0.8f) : new Color(0.8f, 0.2f, 0.2f, 0.8f);
            
            // Рисуем куб, представляющий комнату
            Gizmos.color = roomFillColor;
            Gizmos.DrawCube(room.roomCenter.position, room.roomSize);
            
            Gizmos.color = roomWireColor;
            Gizmos.DrawWireCube(room.roomCenter.position, room.roomSize);
            
            // Рисуем имя комнаты
            #if UNITY_EDITOR
            UnityEditor.Handles.color = roomWireColor;
            UnityEditor.Handles.Label(
                room.roomCenter.position + Vector3.up * (room.roomSize.y * 0.5f + 0.5f), 
                room.roomName,
                new GUIStyle() { normal = new GUIStyleState() { textColor = roomWireColor }, fontStyle = FontStyle.Bold }
            );
            #endif
            
            // Рисуем линии к источникам света в комнате
            if (room.roomLights != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var light in room.roomLights)
                {
                    if (light != null)
                    {
                        Gizmos.DrawLine(room.roomCenter.position, light.transform.position);
                        Gizmos.DrawWireSphere(light.transform.position, 0.2f);
                    }
                }
            }
            
            // Рисуем мини-куб в центре комнаты для лучшей визуализации
            Gizmos.color = Color.white;
            Gizmos.DrawCube(room.roomCenter.position, Vector3.one * 0.3f);
        }
        
        // Визуализируем порталы
        foreach (var connection in portalConnections)
        {
            if (connection == null) continue;
            
            // Визуализация подготовительного портала
            if (connection.preparationPortal != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(connection.preparationPortal.position, portalTriggerRadius);
                
                if (connection.sourceRoom != null && connection.sourceRoom.roomCenter != null)
                {
                    Gizmos.DrawLine(connection.preparationPortal.position, connection.sourceRoom.roomCenter.position);
                }
                
                // Добавляем метку для портала
                #if UNITY_EDITOR
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.Label(
                    connection.preparationPortal.position + Vector3.up * 0.5f, 
                    "Prep Portal",
                    new GUIStyle() { normal = new GUIStyleState() { textColor = Color.cyan }, fontStyle = FontStyle.Bold }
                );
                #endif
            }
            
            // Визуализация переходного портала
            if (connection.transitionPortal != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(connection.transitionPortal.position, portalTriggerRadius);
                
                if (connection.destinationRoom != null && connection.destinationRoom.roomCenter != null)
                {
                    Gizmos.DrawLine(connection.transitionPortal.position, connection.destinationRoom.roomCenter.position);
                }
                
                // Добавляем метку для портала
                #if UNITY_EDITOR
                UnityEditor.Handles.color = Color.magenta;
                UnityEditor.Handles.Label(
                    connection.transitionPortal.position + Vector3.up * 0.5f, 
                    "Trans Portal",
                    new GUIStyle() { normal = new GUIStyleState() { textColor = Color.magenta }, fontStyle = FontStyle.Bold }
                );
                #endif
            }
            
            // Рисуем линию соединения между порталами
            if (connection.preparationPortal != null && connection.transitionPortal != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(connection.preparationPortal.position, connection.transitionPortal.position);
            }
            
            // Добавляем информацию о соединении
            if (connection.sourceRoom != null && connection.destinationRoom != null)
            {
                #if UNITY_EDITOR
                if (connection.preparationPortal != null && connection.transitionPortal != null)
                {
                    Vector3 midPoint = (connection.preparationPortal.position + connection.transitionPortal.position) * 0.5f;
                    UnityEditor.Handles.color = Color.white;
                    UnityEditor.Handles.Label(
                        midPoint + Vector3.up * 0.5f, 
                        $"{connection.sourceRoom.roomName} → {connection.destinationRoom.roomName}",
                        new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontStyle = FontStyle.Bold }
                    );
                }
                #endif
            }
        }
        
        // Если игрок задан, показываем его текущее положение
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, 0.5f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.Label(
                player.position + Vector3.up * 1f, 
                "Player",
                new GUIStyle() { normal = new GUIStyleState() { textColor = Color.blue }, fontStyle = FontStyle.Bold }
            );
            #endif
        }
    }
} 