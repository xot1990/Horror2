using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUi : MonoBehaviour
{
    [SerializeField] private QuestUiElement elementPrefab;
    [SerializeField] private Transform questContainer;
    private float _tickTime;

    private void OnEnable()
    {
        QuestEventBus.fillQuests += FillQuests;
    }

    private void OnDisable()
    {
        QuestEventBus.fillQuests -= FillQuests;
    }

    private void Update()
    {
        _tickTime -= Time.deltaTime;

        if (_tickTime <= 0)
        {
            QuestEventBus.GetTickTime();
            _tickTime = 1;
        }
    }

    private void FillQuests(List<Quest> Q)
    {
        foreach (var quest in Q)
        {
            CreateQuestItem(quest);
        }
    }

    private void CreateQuestItem(Quest Q)
    {
        QuestUiElement E = Instantiate(elementPrefab, questContainer);
        E.FillElement(Q);
    }
}
