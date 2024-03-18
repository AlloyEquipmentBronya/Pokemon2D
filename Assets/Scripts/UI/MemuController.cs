using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MemuController : MonoBehaviour
{
    [SerializeField] GameObject memu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<Text> memuItems;
    //记录第一个位置(选项 默认为0
    int selectedItem = 0;

    private void Awake()
    {
        //使用linq 的tolist方法获得菜单的所有子对象的Text属性
       memuItems= memu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMemu()
    {
        memu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMemu()
    {
        memu.SetActive(false);
    }

    /// <summary>
    /// 菜单选择行为
    /// </summary>
    public void HandleUpdate()
    {
        int prevSelecton = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            selectedItem++;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selectedItem--;
        selectedItem = Mathf.Clamp(selectedItem, 0, memuItems.Count - 1);

        if(prevSelecton!=selectedItem)
        UpdateItemSelection();

        if(Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMemu();
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            CloseMemu();
        }

    } 

    /// <summary>
    /// 更新选中的颜色
    /// </summary>
    public void UpdateItemSelection()
    {
        for (int i = 0; i < memuItems.Count; i++)
        {
            if (selectedItem == i)
                memuItems[i].color =GlobalSettings.i.HighlightedColor;
            else
                memuItems[i].color = Color.black;
        }
    }
}
