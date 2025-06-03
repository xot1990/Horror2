using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestData : ScriptableObject
{
    public string Name;
    public string Discription;
    public QuestData nextQuest;
    
    public virtual Quest CreateQuest()
    {
        return new Quest(this);
    }
}
