using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;

    //判断等待 可以选择
    bool choiceSelected = false;

    /// <summary>
    /// 存放 生成的ChoiceText 集合
    /// </summary>
    List<ChoiceText> choiceTexts;

    int currentChoice;

    /// <summary>
    /// 显示选择项 清理之前那的选项重新加载
    /// </summary>
    /// <param name="choices"></param>
    /// <returns></returns>
    public IEnumerator ShowChoices(List<string> choices,Action<int> onChoicesSelected)
    {
        choiceSelected = false;
        currentChoice = 0;
        gameObject.SetActive(true);

        //删除存在的子对象  之前的选项
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        choiceTexts = new List<ChoiceText>();



        //为生成的每一项赋值文字
        foreach (var choice in choices)
        {
            var choiceTextObj= Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choice;
            choiceTexts.Add(choiceTextObj);
        }
        //等待 为 true 才继续执行下一行
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
            //当当前下标选中下标时 效果变化
            choiceTexts[i].SetSelected(i == currentChoice);
        }

        if (Input.GetKeyDown(KeyCode.Z))
            choiceSelected = true;
    }
}
