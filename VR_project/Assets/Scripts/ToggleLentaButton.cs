using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class ToggleLentaButton : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Объект со скриптом LentaScript")]
    public LentaScript targetLenta;

    [Header("Цвета кнопки")]
    [Tooltip("Цвет когда лента активна (isRunning = true)")]
    public Color activeColor = Color.green;

    [Tooltip("Цвет когда лента неактивна (isRunning = false)")]
    public Color inactiveColor = Color.red;

    private Material buttonMaterial;
    private bool currentState;

    private void Start()
    {
        if (targetLenta == null)
        {
            Debug.LogError("Target LentaScript is not assigned!");
        }

        // Получаем материал кнопки
        buttonMaterial = GetComponent<Renderer>().material;

        // Инициализируем цвет кнопки
        UpdateButtonColor();
    }

    // Вызывается при нажатии на кнопку
    private void HandHoverUpdate(Hand hand)
    {
        // Проверяем нажатие основной кнопки
        if (hand.GetGrabStarting() != GrabTypes.None)
        {
            ToggleLentaState();
        }
    }

    // Обработка клика мышью (для симуляции без VR)
    private void OnMouseDown()
    {
        // Игнорируем клик мыши в VR режиме
        if (Player.instance != null && Player.instance.isActiveAndEnabled)
            return;

        ToggleLentaState();
    }

    private void ToggleLentaState()
    {
        if (targetLenta != null)
        {
            // Меняем состояние на противоположное
            targetLenta.isRunning = !targetLenta.isRunning;
            Debug.Log($"Состояние ленты изменено: {targetLenta.isRunning}");

            // Обновляем цвет кнопки
            UpdateButtonColor();
        }
    }

    private void UpdateButtonColor()
    {
        if (buttonMaterial != null)
        {
            // Устанавливаем цвет в зависимости от состояния
            buttonMaterial.color = targetLenta.isRunning ? activeColor : inactiveColor;
        }
    }

    // Для автоматического обновления цвета при изменении состояния извне
    private void Update()
    {
        if (targetLenta != null && targetLenta.isRunning != currentState)
        {
            currentState = targetLenta.isRunning;
            UpdateButtonColor();
        }
    }
}