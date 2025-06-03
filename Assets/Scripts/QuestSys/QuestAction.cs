using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class QuestAction : Quest
{
    public QuestAction(QuestDataAction D) : base(D)
    {
        questType = QuestType.Action;
        goal = D.ActionCount;
        Name = D.Name;
        NextQuest = D.nextQuest;
        discription = D.Discription;
    }
    
    public override void Done(string T)
    {
        if (T == Name)
        {
            progress++;
        }
        
        base.Done(T);
    }
}
