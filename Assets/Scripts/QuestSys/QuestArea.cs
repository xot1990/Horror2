using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestArea : MonoBehaviour
{
    public QuestData quest;
    public AudioClip clip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestEventBus.GetStartQuest(quest);
            if(clip != null)
                AudioSource.PlayClipAtPoint(clip,transform.position);
            Destroy(gameObject);
        }
    }

}
