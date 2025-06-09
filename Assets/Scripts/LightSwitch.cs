using System.Collections;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public Animator SwitchAnimator; // Аниматор переключателя
    public bool open; // Состояние переключателя (вкл/выкл)
    public bool isUse; // Флаг, что свет включен впервые
    public Transform Player; // Ссылка на игрока
    public float interactionDistance = 15f; // Дистанция для взаимодействия
    public Light[] LightPool; // Массив источников света (используем массив вместо List для оптимизации)

    void Start()
    {
        open = false;
        isUse = false;
    }

    // Метод для взаимодействия с переключателем (вызывается системой квестов)
    public void Use()
    {
        // Проверяем дистанцию до игрока
        if (Player == null || Vector3.Distance(Player.position, transform.position) > interactionDistance)
        {
            Debug.Log("Player too far or not assigned!");
            return;
        }

        if (open)
        {
            StartCoroutine(Off());
        }
        else
        {
            StartCoroutine(On());
        }
    }

    IEnumerator On()
    {
        Debug.Log("You are turning on the light");
        if (SwitchAnimator != null)
        {
            SwitchAnimator.Play("On");
        }
        open = true;
        yield return new WaitForSeconds(0.5f);

        if (!isUse)
        {
            isUse = true;
            QuestEventBus.GetDoneAction("LightOn"); // Вызываем событие
        }

        foreach (var light in LightPool)
        {
            if (light != null)
            {
                light.intensity = 1;
            }
        }
    }

    IEnumerator Off()
    {
        Debug.Log("You are turning off the light");
        if (SwitchAnimator != null)
        {
            SwitchAnimator.Play("Off");
        }
        open = false;
        yield return new WaitForSeconds(0.5f);

        foreach (var light in LightPool)
        {
            if (light != null)
            {
                light.intensity = 0;
            }
        }
    }
}