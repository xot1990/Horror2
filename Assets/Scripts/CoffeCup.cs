using UnityEngine;

public class CoffeeFillSingleParticle : MonoBehaviour
{
    public Transform bottomObject; // Объект дна, также представляющий поверхность жидкости
    public float fillSpeed = 0.001f; // Скорость наполнения за частицу
    public float maxHeight; // Максимальная высота жидкости (по оси Z)
    public float minHeight = 0f; // Минимальная высота жидкости (по оси Z)
    public float spillSpeed = 0.002f; // Скорость уменьшения высоты при проливании
    public float spillAngleThreshold = 30f; // Порог угла наклона для проливания (градусы)
    public float startScale = 0.00202f; // Стартовый масштаб дна
    public float maxScale = 0.00288f; // Масштаб дна при полном заполнении
    private float currentHeight = 0f; // Текущий уровень жидкости (по оси Z)
    private bool isStarted = false; // Флаг для активации поверхности
    private bool isPickedUp = false; // Флаг, что стаканчик поднят
    private bool isLidded = false; // Флаг, что крышечка установлена
    private Collider cupCollider; // Коллайдер стаканчика
    private Rigidbody cupRigidbody; // Rigidbody стаканчика
    private MeshRenderer bottomRenderer; // MeshRenderer дна
    private readonly Vector3 verticalEulerAngles = new Vector3(-90f, 0f, 0f); // Вертикальное положение стакана

    public float FillLevel => currentHeight / maxHeight; // Показатель заполненности (0–1)

    void Start()
    {
        cupCollider = GetComponent<Collider>();
        cupRigidbody = GetComponent<Rigidbody>();
        if (cupRigidbody == null)
        {
            cupRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        if (bottomObject == null)
        {
            Debug.LogError("Bottom object is not assigned in the inspector!");
        }
        else
        {
            // Получаем MeshRenderer и изначально выключаем
            bottomRenderer = bottomObject.GetComponent<MeshRenderer>();
            if (bottomRenderer == null)
            {
                Debug.LogError("Bottom object does not have a MeshRenderer!");
            }
            else
            {
                bottomRenderer.enabled = false;
            }
            // Устанавливаем начальный масштаб
            bottomObject.localScale = new Vector3(startScale, startScale, startScale);
        }
        // Убедимся, что коллайдер стакана настроен как триггер
        if (cupCollider != null)
        {
            cupCollider.isTrigger = true;
        }
    }

    void Update()
    {
        if (isPickedUp || isLidded)
        {
            return;
        }

        // Проверяем наклон стаканчика для проливания (только по оси X)
        if (isStarted && !isLidded)
        {
            float tiltAngleX = Mathf.Abs(transform.rotation.eulerAngles.x - verticalEulerAngles.x);
            // Нормализуем угол к диапазону 0–180 градусов
            if (tiltAngleX > 180f) tiltAngleX = 360f - tiltAngleX;
            bool isSpilling = tiltAngleX > spillAngleThreshold;

            if (isSpilling && currentHeight > minHeight)
            {
                // Уменьшаем высоту при проливании
                currentHeight = Mathf.Max(currentHeight - spillSpeed * Time.deltaTime, minHeight);
                Debug.Log($"Spilling coffee: tiltAngleX={tiltAngleX}, currentHeight={currentHeight}, FillLevel={FillLevel}");
            }

            // Скрываем поверхность, если стакан пуст
            if (currentHeight <= minHeight && isStarted)
            {
                if (bottomRenderer != null)
                {
                    bottomRenderer.enabled = false;
                }
                isStarted = false;
                Debug.Log("Coffee cup is empty, hiding surface");
            }
        }

        // Обновляем позицию и масштаб дна
        if (isStarted)
        {
            if (bottomObject != null)
            {
                bottomObject.localPosition = new Vector3(
                    bottomObject.localPosition.x,
                    bottomObject.localPosition.y,
                    currentHeight
                );

                // Обновляем масштаб на основе FillLevel
                float scale = Mathf.Lerp(startScale, maxScale, FillLevel);
                bottomObject.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (isLidded) return; // Игнорируем, если крышечка уже установлена

        if (other.gameObject.CompareTag("Lid"))
        {
            Debug.Log($"Lid detected in trigger: {other.gameObject.name}");
            PlaceLid(other.gameObject);
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (isLidded || isPickedUp) return; // Игнорируем частицы, если стаканчик закрыт или поднят

        // Активируем поверхность при первом попадании
        if (!isStarted)
        {
            isStarted = true;
            if (bottomRenderer != null)
            {
                bottomRenderer.enabled = true;
            }
            Debug.Log("Started filling coffee cup");
        }

        // Увеличиваем уровень жидкости по оси Z
        currentHeight = Mathf.Min(currentHeight + fillSpeed, maxHeight);
        Debug.Log($"Filling coffee: currentHeight={currentHeight}, FillLevel={FillLevel}");
    }

    public void PickUp()
    {
        isPickedUp = true;
        cupCollider.enabled = false; // Отключаем коллайдер
        Debug.Log($"Picked up cup: {gameObject.name}");
    }

    public void Drop()
    {
        isPickedUp = false;
        cupCollider.enabled = !isLidded; // Включаем коллайдер, если нет крышечки
        Debug.Log($"Dropped cup: {gameObject.name}");
    }

    public void PlaceLid(GameObject lid)
    {
        if (lid == null)
        {
            Debug.LogError("Lid object is null in PlaceLid!");
            return;
        }

        isLidded = true;
        lid.transform.SetParent(transform); // Прикрепляем крышечку к стаканчику
        lid.transform.localPosition = new Vector3(0, 0, lid.transform.localPosition.z); // Позиция над стаканчиком по Z
        lid.transform.localRotation = Quaternion.identity; // Сбрасываем вращение

        // Отключаем физику и возможность взаимодействия
        var lidRigidbody = lid.GetComponent<Rigidbody>();
        if (lidRigidbody != null)
        {
            lidRigidbody.isKinematic = true; // Делаем Rigidbody кинематическим
            lidRigidbody.useGravity = false; // Отключаем гравитацию
        }
        else
        {
            Debug.LogWarning($"No Rigidbody on lid: {lid.name}");
        }

        var lidCollider = lid.GetComponent<Collider>();
        if (lidCollider != null)
        {
            lidCollider.enabled = false; // Отключаем коллайдер
        }
        else
        {
            Debug.LogWarning($"No Collider on lid: {lid.name}");
        }

        // Удаляем тег "Lid", чтобы предотвратить повторное взятие
        lid.tag = "Untagged";
        Debug.Log($"Placed lid on cup: {lid.name}, FillLevel={FillLevel}, Parent={lid.transform.parent.name}");
    }
}