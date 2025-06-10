using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDoneArea : MonoBehaviour
{
    public QuestData quest;
    public AudioClip clip;
    public AudioSource source;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestEventBus.GetDoneAction(quest.Name);
            if(clip != null)
                AudioSource.PlayClipAtPoint(clip,other.transform.position);
            
            if(source != null)
                source.Play();
            
            gameObject.SetActive(false);
        }
    }

}
