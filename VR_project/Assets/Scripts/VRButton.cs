using UnityEngine;

public class VRButton : MonoBehaviour
{
    [Header("Conveyor Settings")]
    public LentaScript conveyorBelt; // Ссылка на скрипт ленты

    [Header("Visual Settings")]
    public Material normalMaterial; // Обычный материал
    public Material highlightMaterial; // Материал при наведении
    public Material pressedMaterial; // Материал при нажатии

    private Renderer buttonRenderer;
    private bool isHighlighted = false;
    private bool isPressed = false;
    private float pressTimer = 0f;
    private const float pressDuration = 0.2f; // Время визуальной реакции на нажатие

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer == null)
        {
            Debug.LogError("ButtonRenderer not found! Add a Renderer component to the button.");
            enabled = false;
            return;
        }

        // Устанавливаем начальный материал
        buttonRenderer.material = normalMaterial;

        // Автоматический поиск ленты, если не установлена
        if (conveyorBelt == null)
        {
            conveyorBelt = FindObjectOfType<LentaScript>();
            if (conveyorBelt == null)
            {
                Debug.LogWarning("ConveyorBelt reference not set and not found in scene.");
            }
        }
    }

    void Update()
    {
        // Обработка визуальной обратной связи для нажатия
        if (isPressed)
        {
            pressTimer += Time.deltaTime;
            if (pressTimer >= pressDuration)
            {
                isPressed = false;
                buttonRenderer.material = isHighlighted ? highlightMaterial : normalMaterial;
            }
        }
    }

    // Вызывается при наведении луча на кнопку
    public void OnRaycastEnter()
    {
        if (isHighlighted) return;

        isHighlighted = true;
        buttonRenderer.material = highlightMaterial;
    }

    // Вызывается при уходе луча с кнопки
    public void OnRaycastExit()
    {
        if (!isHighlighted) return;

        isHighlighted = false;
        buttonRenderer.material = normalMaterial;
    }

    // Вызывается при нажатии на кнопку
    public void OnPress()
    {
        if (isPressed) return;

        isPressed = true;
        pressTimer = 0f;
        buttonRenderer.material = pressedMaterial;

        // Переключаем состояние конвейера
        if (conveyorBelt != null)
        {
            conveyorBelt.isRunning = !conveyorBelt.isRunning;
            Debug.Log($"Conveyor state changed to: {conveyorBelt.isRunning}");
        }
        else
        {
            Debug.LogWarning("ConveyorBelt reference not set!");
        }
    }
}