using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDoneArea : MonoBehaviour
{
    public QuestData quest;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestEventBus.GetDoneAction(quest.Name);
            gameObject.SetActive(false);
        }
    }

}
