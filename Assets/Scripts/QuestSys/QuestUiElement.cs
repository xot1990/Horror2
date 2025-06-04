using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class QuestUiElement : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private Image questIcon;
    [SerializeField] private List<Sprite> Icons;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text discription;
    
    private Quest quest;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        QuestEventBus.updateQuestUi += UpdateProgress;
        QuestEventBus.doneQuest += DoneQuest;
    }

    private void OnDisable()
    {
        QuestEventBus.updateQuestUi -= UpdateProgress;
        QuestEventBus.doneQuest -= DoneQuest;
    }

    public void FillElement(Quest Q)
    {
        quest = Q;

        discription.text = quest.discription;
        
        switch (Q.questType)
        {
            case Quest.QuestType.Kill:
                questIcon.sprite = Icons[0];
                break;
            case Quest.QuestType.Time:
                questIcon.sprite = Icons[1];
                break;
            default:
                questIcon.sprite = Icons[0];
                break;
        }
        
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        progressBar.fillAmount = quest.progress / (float)quest.goal;
        progressText.text = quest.progress + "/" + quest.goal;
    }

    private void DoneQuest(Quest Q)
    {
        
        if(Q.Name == quest.Name)
            _animator.Play("Done");
    }

    public void DestroyObj()
    {
        Destroy(gameObject);
    }
}
