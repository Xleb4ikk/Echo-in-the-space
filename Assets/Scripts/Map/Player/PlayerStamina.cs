using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 1000f;
    public float currentStamina;
    public float staminaDrain = 1f;      // сколько тратится в секунду при спринте
    public float staminaRegen = 10f;      // сколько восстанавливается в секунду при движении
    public float staminaRegenIdle = 25f;  // сколько восстанавливается в секунду при простое
    public Player player;                 // ссылка на скрипт Player
    public Slider staminaSlider;          // ссылка на UI Slider

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStamina = maxStamina;
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        else
        {
            // Если игрок стоит на месте, восстанавливаем быстрее
            if (player != null && player.MoveInput == Vector2.zero)
            {
                currentStamina += staminaRegenIdle * Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegen * Time.deltaTime;
            }
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }

        staminaSlider.value = currentStamina;
    }
}
