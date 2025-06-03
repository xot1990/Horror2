using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestArea : MonoBehaviour
{
    public QuestData quest;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestEventBus.GetStartQuest(quest);
            Destroy(gameObject);
        }
    }

}
