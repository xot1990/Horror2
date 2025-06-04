using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLogic : MonoBehaviour
{
    private List<Quest> _quests = new();
    private delegate bool CheckQuest(ref float target,float value);

    private CheckQuest Quest;

    private void OnEnable()
    {
        QuestEventBus.tickTime += ObserveTimeEvent;
        QuestEventBus.enemyDie += ObserveKillEvent;
        QuestEventBus.doneAction += ObserveActionEvent;
        QuestEventBus.doneQuest += QuestDone;
        QuestEventBus.startQuest += StartQuest;
    }

    private void OnDisable()
    {
        QuestEventBus.tickTime -= ObserveTimeEvent;
        QuestEventBus.enemyDie -= ObserveKillEvent;
        QuestEventBus.doneAction -= ObserveActionEvent;
        QuestEventBus.doneQuest -= QuestDone;
        QuestEventBus.startQuest -= StartQuest;
    }

    private void ObserveTimeEvent()
    {
        foreach (var q in _quests)
        {
            if (q.questType == global::Quest.QuestType.Time)
            {
                Enemy E = null;
                q.Done(E);
            }
        }
    }
    
    private void ObserveActionEvent(string T)
    {
        foreach (var q in _quests)
        {
            if (q.questType == global::Quest.QuestType.Action)
            {
                q.Done(T);
            }
        }
    }
    
    private void ObserveKillEvent(Enemy enemy)
    {
        foreach (var q in _quests)
        {
            if (q.questType == global::Quest.QuestType.Kill)
            {
                q.Done(enemy);
            }
        }
    }

    private void QuestDone(Quest Q)
    {
        _quests.Remove(_quests.Find(X => X.Name == Q.Name));
        _quests.Add(Q.NextQuest?.CreateQuest());
        QuestEventBus.GetFillQuests(_quests);
    }

    private void StartQuest(QuestData Q)
    {
        _quests.Add(Q.CreateQuest());
        QuestEventBus.GetFillQuests(_quests);
    }
}
