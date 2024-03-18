using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;

    //�жϵȴ� ����ѡ��
    bool choiceSelected = false;

    /// <summary>
    /// ��� ���ɵ�ChoiceText ����
    /// </summary>
    List<ChoiceText> choiceTexts;

    int currentChoice;

    /// <summary>
    /// ��ʾѡ���� ����֮ǰ�ǵ�ѡ�����¼���
    /// </summary>
    /// <param name="choices"></param>
    /// <returns></returns>
    public IEnumerator ShowChoices(List<string> choices,Action<int> onChoicesSelected)
    {
        choiceSelected = false;
        currentChoice = 0;
        gameObject.SetActive(true);

        //ɾ�����ڵ��Ӷ���  ֮ǰ��ѡ��
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        choiceTexts = new List<ChoiceText>();



        //Ϊ���ɵ�ÿһ�ֵ����
        foreach (var choice in choices)
        {
            var choiceTextObj= Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choice;
            choiceTexts.Add(choiceTextObj);
        }
        //�ȴ� Ϊ true �ż���ִ����һ��
        yield return new WaitUntil(() => choiceSelected == true);

        onChoicesSelected?.Invoke(currentChoice);
        gameObject.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentChoice;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentChoice;

        currentChoice = Mathf.Clamp(currentChoice, 0, choiceTexts.Count - 1);

        for (int i = 0; i < choiceTexts.Count; i++)
        {
            //����ǰ�±�ѡ���±�ʱ Ч���仯
            choiceTexts[i].SetSelected(i == currentChoice);
        }

        if (Input.GetKeyDown(KeyCode.Z))
            choiceSelected = true;
    }
}
