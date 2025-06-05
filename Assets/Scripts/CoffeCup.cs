using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeCup : MonoBehaviour
{
    public ParticleSystem liquidSurface; // Particle System для поверхности жидкости
    public float fillSpeed = 0.001f; // Скорость наполнения за частицу
    public float maxHeight; // Максимальная высота жидкости
    public float overflowSpeed = 2f; // Скорость частиц при переливании
    public float stopOverflowDelay = 0.5f; // Задержка перед остановкой переливания (в секундах)
    private float currentHeight = 0f;
    private bool isOverflowing = false;
    private float lastParticleHitTime; // Время последнего попадания частицы
    private bool isReceivingParticles = false;

    void Start()
    {
        if (liquidSurface != null)
        {
            var emission = liquidSurface.emission;
            emission.rateOverTime = 50; // Начальная эмиссия для поверхности
            var collision = liquidSurface.collision;
            collision.enabled = true; // Включаем коллизию для удержания частиц
            var force = liquidSurface.forceOverLifetime;
            force.enabled = false; // Изначально отключаем силу для переливания
            lastParticleHitTime = Time.time;
        }
    }

    void Update()
    {
        // Проверяем, продолжают ли поступать частицы
        if (isReceivingParticles && Time.time - lastParticleHitTime > stopOverflowDelay)
        {
            // Если частицы перестали поступать, останавливаем переливание
            isReceivingParticles = false;
            if (isOverflowing)
            {
                StopOverflow();
            }
        }

        // Плавное обновление позиции эмиттера
        liquidSurface.transform.localPosition = new Vector3(
            liquidSurface.transform.localPosition.x,
            currentHeight,
            liquidSurface.transform.localPosition.z
        );
    }

    void OnParticleCollision(GameObject other)
    {
        // Отмечаем, что частицы поступают
        isReceivingParticles = true;
        lastParticleHitTime = Time.time;

        if (!isOverflowing)
        {
            // Увеличиваем уровень жидкости
            currentHeight += fillSpeed;

            // Проверяем, достиг ли уровень максимума
            if (currentHeight >= maxHeight)
            {
                StartOverflow();
            }
        }
    }

    void StartOverflow()
    {
        isOverflowing = true;
        var emission = liquidSurface.emission;
        emission.rateOverTime = 100; // Увеличиваем эмиссию для переливания
        var collision = liquidSurface.collision;
        collision.enabled = false; // Отключаем коллизию для переливания
        var force = liquidSurface.forceOverLifetime;
        force.enabled = true;
        force.y = -overflowSpeed; // Задаём силу для стекания частиц
    }

    void StopOverflow()
    {
        isOverflowing = false;
        var emission = liquidSurface.emission;
        emission.rateOverTime = 50; // Возвращаем нормальную эмиссию
        var collision = liquidSurface.collision;
        collision.enabled = true; // Включаем коллизию обратно
        var force = liquidSurface.forceOverLifetime;
        force.enabled = false; // Отключаем силу для переливания
    }
}
