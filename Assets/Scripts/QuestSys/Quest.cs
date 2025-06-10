using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Quest
{
    public enum QuestType
    {
        Time,
        Kill,
        Action
    }

    public string Name;
    public string discription;
    public int progress;
    public int goal;
    public QuestType questType;
    public QuestData NextQuest;
    public delegate void ActionMethod();

    public ActionMethod method;

    public Quest(QuestData D) { }
    
    public virtual void Done(Enemy T)
    {
        QuestEventBus.GetUpdateQuestUi();
        CheckDone();
    }
    
    public virtual void Done(string T)
    {
        QuestEventBus.GetUpdateQuestUi();
        CheckDone();
    }

    private void CheckDone()
    {
        if(progress >= goal)
            QuestEventBus.GetDoneQuest(this);
    }
}
