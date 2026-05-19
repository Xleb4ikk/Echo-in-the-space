using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator m_Animator;
    public bool isOpen;
    public AudioSource audioSource;
    private Collider doorCollider; // Добавляем для управления коллайдером
    private float interactionCooldown = 0.5f; // Задержка для предотвращения спама
    private float lastInteractionTime;

    void Start()
    {
        m_Animator = GetComponent<Animator>(); // Автоматическая инициализация
        doorCollider = GetComponent<Collider>(); // Инициализация коллайдера
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        m_Animator.SetBool("isOpen", isOpen);
        
        if (doorCollider != null)
        {
            doorCollider.enabled = true; // Всегда включаем коллайдер для взаимодействия
        }
    }

    public string GetDescription()
    {
        if (isOpen) return "Нажмите [E] чтобы <color=red>закрыть</color> дверь";
        return "Нажмите [E] чтобы <color=green>открыть</color> дверь";
    }

    public void Interact()
    {
        if (Time.time - lastInteractionTime < interactionCooldown) return; // Проверка задержки
        lastInteractionTime = Time.time;

        isOpen = !isOpen;
        m_Animator.SetBool("isOpen", isOpen); // Используем правильный параметр
        
        // doorCollider.enabled = !isOpen; // ЗАКОММЕНТИРОВАНО: не отключаем коллайдер, чтобы взаимодействие работало всегда
        if (doorCollider != null)
        {
            Debug.Log($"Collider remains enabled: true (isOpen: {isOpen})"); // Лог для отладки
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log("Audio played");
        }

        Debug.Log($"Door Interact called, isOpen: {isOpen}, Animator isOpen: {m_Animator.GetBool("isOpen")}");
    }
}