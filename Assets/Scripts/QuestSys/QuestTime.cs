using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTime : Quest
{
    public QuestTime(QuestDataTimes D) : base(D)
    {
        questType = QuestType.Time;
        goal = D.seconds;
    }
    
    public override void Done(Enemy T)
    {
        progress++;
        
        base.Done(T);
    }
}
