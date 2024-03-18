using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countTxt;
    [SerializeField] Text priceTxt;

    bool selected;
    int currentCount;

    int maxCount;
    float pricePerUnit;

    /// <summary>
    /// 显示多选出售
    /// </summary>
    /// <param name="maxCount">获得选中项的最大数目</param>
    /// <param name="pricePerUnit">单价</param>
    /// <returns></returns>
    public IEnumerator ShowSelector(int maxCount,float pricePerUnit,
        Action<int> OnCountSelected)
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;

        selected = false;

        gameObject.SetActive(true);
        SetValues();

        yield return new WaitUntil(() => selected == true);

        OnCountSelected?.Invoke(currentCount);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        int prevCount = currentCount;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            ++currentCount;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            --currentCount;
        currentCount = Mathf.Clamp(currentCount, 1, maxCount);

        if (currentCount != prevCount)
            SetValues();

        if (Input.GetKeyDown(KeyCode.Z))
            selected = true;
    }

    void SetValues()
    {
        countTxt.text = "x " + currentCount;
        priceTxt.text = "￥ " + pricePerUnit * currentCount;
    }
}
