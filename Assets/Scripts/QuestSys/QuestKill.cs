using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class QuestKill : Quest
{
    public List<Enemy> targets = new();

    public QuestKill(QuestDataKills D) : base(D)
    {
        questType = QuestType.Kill;
        goal = D.kills;

        foreach (var enemy in D.enemyType)
        {
            targets.Add(enemy);
        }
    }
    
    public override void Done(Enemy T)
    {
        if (targets.Any(t => T.GetType() == t.GetType()))
        {
            progress++;
        }
        
        base.Done(T);
    }
}
