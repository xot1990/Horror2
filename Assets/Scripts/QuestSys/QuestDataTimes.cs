using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class QuestDataTimes : QuestData
{
    public int seconds;
    
    public override Quest CreateQuest()
    {
        return new QuestTime(this);
    }
}
