using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLogic : MonoBehaviour
{
    private List<Quest> _quests = new();
    public List<QuestDoneArea> Areas = new();
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
        for (int i = 0; i < _quests.Count; i++)
        {
            if (_quests[i].questType == global::Quest.QuestType.Time)
            {
                Enemy E = null;
                _quests[i].Done(E);
            }
        }
    }
    
    private void ObserveActionEvent(string T)
    {
        for (int i = 0; i < _quests.Count; i++)
        {
            if (_quests[i].questType == global::Quest.QuestType.Action)
            {
                _quests[i].Done(T);
            }
        }
        
    }
    
    private void ObserveKillEvent(Enemy enemy)
    {
        for (int i = 0; i < _quests.Count; i++)
        {
            if (_quests[i].questType == global::Quest.QuestType.Kill)
            {
                _quests[i].Done(enemy);
            }
        }
    }

    private void QuestDone(Quest Q)
    {
        _quests.Remove(_quests.Find(X => X.Name == Q.Name));
        StartQuest(Q.NextQuest);
    }

    private void StartQuest(QuestData Q)
    {
        global::Quest q = Q.CreateQuest();
        _quests.Add(q);
        Q.ActionQuest();
        
        QuestDoneArea area = Areas.Find(X => X.quest == Q);

        if (area != null)
        {
            area.gameObject.SetActive(true);
        }
        
        QuestEventBus.GetFillQuests(_quests);
    }
}
