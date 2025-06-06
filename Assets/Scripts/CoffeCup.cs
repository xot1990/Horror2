using UnityEngine;

public class CoffeeFillSingleParticle : MonoBehaviour
{
    public ParticleSystem liquidSurface; // Particle System для поверхности жидкости
    public float fillSpeed = 0.001f; // Скорость наполнения за частицу
    public float maxHeight; // Максимальная высота жидкости (по оси Z)
    public float overflowSpeed = 2f; // Скорость частиц при переливании
    public float stopOverflowDelay = 0.5f; // Задержка перед остановкой переливания
    private float currentHeight = 0f; // Текущий уровень жидкости (по оси Z)
    private bool isOverflowing = false;
    private float lastParticleHitTime; // Время последнего попадания частицы
    private bool isReceivingParticles = false;
    private bool isStarted = false; // Флаг для активации Particle System
    private bool isPickedUp = false; // Флаг, что стаканчик поднят
    private bool isLidded = false; // Флаг, что крышечка установлена
    private Collider cupCollider; // Коллайдер стаканчика
    private Rigidbody cupRigidbody; // Rigidbody для отключения физики при подборе

    void Start()
    {
        cupCollider = GetComponent<Collider>();
        cupRigidbody = GetComponent<Rigidbody>();
        if (cupRigidbody == null)
        {
            cupRigidbody = gameObject.AddComponent<Rigidbody>();
            cupRigidbody.isKinematic = true; // Изначально без физики
        }
        if (liquidSurface != null)
        {
            liquidSurface.Stop(); // Изначально выключаем Particle System
            var emission = liquidSurface.emission;
            emission.rateOverTime = 50; // Начальная эмиссия
            var collision = liquidSurface.collision;
            collision.enabled = true; // Коллизия для удержания частиц
            var force = liquidSurface.forceOverLifetime;
            force.enabled = false; // Сила для переливания выключена
            lastParticleHitTime = Time.time;
        }
    }

    void Update()
    {
        if (isPickedUp || isLidded)
        {
            // Если стаканчик поднят или закрыт, отключаем переливание
            if (isOverflowing)
            {
                StopOverflow();
            }
            return;
        }

        // Проверяем, поступают ли частицы
        if (isReceivingParticles && Time.time - lastParticleHitTime > stopOverflowDelay)
        {
            isReceivingParticles = false;
            if (isOverflowing)
            {
                StopOverflow();
            }
        }

        // Обновляем позицию эмиттера по оси Z
        if (isStarted)
        {
            liquidSurface.transform.localPosition = new Vector3(
                liquidSurface.transform.localPosition.x,
                liquidSurface.transform.localPosition.y,
                currentHeight
            );
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (isLidded || isPickedUp) return; // Игнорируем частицы, если стаканчик закрыт или поднят

        // Активируем Particle System при первом попадании
        if (!isStarted)
        {
            isStarted = true;
            liquidSurface.Play();
        }

        // Отмечаем, что частицы поступают
        isReceivingParticles = true;
        lastParticleHitTime = Time.time;

        if (!isOverflowing)
        {
            // Увеличиваем уровень жидкости по оси Z
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
        collision.enabled = false; // Отключаем коллизию для стекания
        var force = liquidSurface.forceOverLifetime;
        force.enabled = true;
        force.z = -overflowSpeed; // Сила вниз по оси Z
    }

    void StopOverflow()
    {
        isOverflowing = false;
        var emission = liquidSurface.emission;
        emission.rateOverTime = 50; // Возвращаем нормальную эмиссию
        var collision = liquidSurface.collision;
        collision.enabled = true; // Включаем коллизию
        var force = liquidSurface.forceOverLifetime;
        force.enabled = false; // Отключаем силу
    }

    public void PickUp()
    {
        isPickedUp = true;
        cupCollider.enabled = false; // Отключаем коллайдер
        cupRigidbody.isKinematic = true; // Отключаем физику
    }

    public void Drop()
    {
        isPickedUp = false;
        cupCollider.enabled = !isLidded; // Включаем коллайдер, если нет крышечки
        cupRigidbody.isKinematic = false; // Включаем физику
    }

    public void PlaceLid(GameObject lid)
    {
        isLidded = true;
        cupCollider.enabled = false; // Отключаем коллайдер
        lid.transform.SetParent(transform); // Прикрепляем крышечку к стаканчику
        lid.transform.localPosition = new Vector3(0, 0, maxHeight + 0.05f); // Позиция над стаканчиком по Z
        lid.transform.localRotation = Quaternion.identity; // Сбрасываем вращение
        var lidCollider = lid.GetComponent<Collider>();
        if (lidCollider != null) lidCollider.enabled = false; // Отключаем коллайдер крышечки
        var lidRigidbody = lid.GetComponent<Rigidbody>();
        if (lidRigidbody != null) lidRigidbody.isKinematic = true; // Отключаем физику крышечки
    }
}