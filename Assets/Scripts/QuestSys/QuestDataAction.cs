using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class QuestDataAction : QuestData
{
    public int ActionCount;
    
    public override Quest CreateQuest()
    {
        return new QuestAction(this);
    }
    
    
}
