using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class QuestDataAction : QuestData
{
    public int ActionCount;
    public bool isNPC;

    public override void ActionQuest()
    {
        if (isNPC)
        {
            QuestEventBus.GetSpawnNPC();
        }
    }

    public override Quest CreateQuest()
    {
        return new QuestAction(this);
    }
    
    
}
