using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class CinematicHorrorEffect : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera; // Виртуальная камера
    [SerializeField] private Transform cameraEndPosition; // Позиция камеры для финала
    [SerializeField] private Image fadeImage; // UI Image для затемнения
    [SerializeField] private PostProcessVolume postProcessVolume; // Post-Processing Volume для эффекта молнии
    [SerializeField] private AudioClip lightningSound; // Звук молнии
    [SerializeField] private float lightningDuration = 0.1f; // Длительность вспышки
    [SerializeField] private float delayBetweenLightning = 0.5f; // Задержка между вспышками
    [SerializeField] private float fadeDuration = 2f; // Длительность затемнения
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Название сцены главного меню
    [SerializeField] private string npcTag = "NPC"; // Тег для поиска NPC

    private bool isCinematicTriggered = false;
    private Bloom bloomEffect; // Эффект Bloom для вспышки молнии

    private void Start()
    {
        // Инициализация UI для затемнения
        fadeImage.color = new Color(0, 0, 0, 0);

        // Настройка Post-Processing для эффекта молнии
        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out Bloom bloom))
        {
            bloomEffect = bloom;
            bloomEffect.intensity.value = 0; // Изначально выключен
        }
    }

    private void OnEnable()
    {
        QuestEventBus.finalAction += StartCinematic;
    }

    private void OnDisable()
    {
        QuestEventBus.finalAction -= StartCinematic;
    }

    // Вызывается через триггер или другое событие
    public void StartCinematic()
    {
        if (isCinematicTriggered) return;
        isCinematicTriggered = true;

        // Отключаем управление игроком
        DisablePlayerControls();

        // Находим NPC
        GameObject npc = GameObject.FindGameObjectWithTag(npcTag);
        if (npc == null)
        {
            Debug.LogError("NPC с тегом " + npcTag + " не найден!");
            return;
        }

        // Настраиваем камеру
        SetupCinematicCamera(npc.transform);

        // Запускаем кинематографическую последовательность
        StartCoroutine(CinematicSequence(npc.GetComponent<NPCController>()));
    }

    private void DisablePlayerControls()
    {
        // Отключаем CharacterController (или ваш компонент управления)
        var playerController = FindObjectOfType<PlayerInput>();
        if (playerController != null) playerController.enabled = false;
        
    }

    private void SetupCinematicCamera(Transform npcTransform)
    {
        // Отключаем следование за игроком
        cinemachineCamera.Follow = null;
        cinemachineCamera.LookAt = npcTransform;

        // Перемещаем камеру в статичную позицию
        cinemachineCamera.transform.position = cameraEndPosition.position;
        cinemachineCamera.transform.rotation = cameraEndPosition.rotation;

        // Настраиваем FOV (опционально)
        cinemachineCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = 60f;
    }

    private IEnumerator CinematicSequence(NPCController npcController)
    {
        // Ждем, пока NPC дойдет до нужной точки (настройте по необходимости)
        yield return new WaitForSeconds(2f);

        // Первая вспышка молнии (превращение в монстра)
        yield return StartCoroutine(PlayLightningEffect());
        npcController.TriggerMetamorphosis(true); // Превращение в монстра

        // Задержка между вспышками
        yield return new WaitForSeconds(delayBetweenLightning);

        // Вторая вспышка молнии (возврат в человека)
        yield return StartCoroutine(PlayLightningEffect());
        npcController.TriggerMetamorphosis(false); // Возврат в человека

        // Затемнение экрана
        yield return StartCoroutine(FadeOut());

        // Загрузка главного меню
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator PlayLightningEffect()
    {
        // Проигрываем звук молнии
        if (lightningSound != null)
            AudioSource.PlayClipAtPoint(lightningSound, Camera.main.transform.position, 1f);

        // Включаем эффект молнии (Bloom)
        if (bloomEffect != null)
        {
            bloomEffect.intensity.value = 10f; // Яркая вспышка
            yield return new WaitForSeconds(lightningDuration);
            bloomEffect.intensity.value = 0f; // Выключаем
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0, 0, 0, 1);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeImage.color = endColor;
    }

    // Вызов через триггер (например, когда игрок или NPC входит в зону)
    
}