using System.Collections;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public Animator SwitchAnimator; // �������� �������������
    public bool open; // ��������� ������������� (���/����)
    public bool isUse; // ����, ��� ���� ������� �������
    public Transform Player; // ������ �� ������
    public float interactionDistance = 15f; // ��������� ��� ��������������
    public Light[] LightPool; // ������ ���������� ����� (���������� ������ ������ List ��� �����������)

    void Start()
    {
        open = false;
        isUse = false;
    }

    // ����� ��� �������������� � �������������� (���������� �������� �������)
    public void Use()
    {
        // ��������� ��������� �� ������
        if (Player == null || Vector3.Distance(Player.position, transform.position) > interactionDistance)
        {
            Debug.Log("Player too far or not assigned!");
            return;
        }

        if (open)
        {
            StartCoroutine(Off());
        }
        else
        {
            StartCoroutine(On());
        }
    }

    IEnumerator On()
    {
        Debug.Log("You are turning on the light");
        if (SwitchAnimator != null)
        {
            SwitchAnimator.Play("On");
        }
        open = true;
        yield return new WaitForSeconds(0.5f);

        if (!isUse)
        {
            isUse = true;
            QuestEventBus.GetDoneAction("LightOn"); // �������� �������
        }

        foreach (var light in LightPool)
        {
            if (light != null)
            {
                light.intensity = 1;
            }
        }
    }

    IEnumerator Off()
    {
        Debug.Log("You are turning off the light");
        if (SwitchAnimator != null)
        {
            SwitchAnimator.Play("Off");
        }
        open = false;
        yield return new WaitForSeconds(0.5f);

        foreach (var light in LightPool)
        {
            if (light != null)
            {
                light.intensity = 0;
            }
        }
    }
}