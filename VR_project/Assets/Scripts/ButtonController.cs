using UnityEngine;
using System;

public class ButtonController : MonoBehaviour
{
    [Header("Conveyor Settings")]
    public LentaScript conveyorBelt; // Ссылка на скрипт конвейерной ленты

    [Header("Button Settings")]
    public float pressDepth = 0.01f; // Глубина нажатия кнопки
    private bool isPressed = false;

    void Start()
    {
    }

    [Header("Interaction Settings")]
    public float interactionDistance = 2f; // Максимальное расстояние взаимодействия

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Проверяем дистанцию взаимодействия
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                // Проверяем что это наша кнопка
                if (hit.collider.gameObject == gameObject ||
                    hit.collider.transform.IsChildOf(transform))
                {
                    PressButton();
                }
            }
        }
    }

    private void PressButton()
    {
        if (isPressed) ReleaseButton();
        isPressed = true;

        // Переключаем состояние конвейера
        if (conveyorBelt != null)
        {
            conveyorBelt.isRunning = !conveyorBelt.isRunning;
        }
    }

    private void ReleaseButton()
    {
        if (!isPressed) return;
        isPressed = false;
    }
}