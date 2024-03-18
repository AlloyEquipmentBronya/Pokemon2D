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
    //��¼��һ��λ��(ѡ�� Ĭ��Ϊ0
    int selectedItem = 0;

    private void Awake()
    {
        //ʹ��linq ��tolist������ò˵��������Ӷ����Text����
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
    /// �˵�ѡ����Ϊ
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
    /// ����ѡ�е���ɫ
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
