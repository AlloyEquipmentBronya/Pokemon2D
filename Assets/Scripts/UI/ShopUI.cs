using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{

    //子对象 物品 UI合集
    [SerializeField] GameObject itemList;
    //为 UI赋值的物品槽
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    List<ItemSlotUI> slotUIList;
    Action<ItemBase> onItemSelected;
    Action onBack;

    int selectedItem = 0;

    /// <summary>
    /// 添加的货物
    /// </summary>
    [SerializeField] List<ItemBase> availableItem;


    const int itemsInViewport = 10;
    RectTransform itemListRect;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }


    /// <summary>
    /// 显示商店货物列表
    /// </summary>
    /// <param name="availableItem">可购买/选择的物品列表</param>
    /// <param name="onItemSelected">事件选中物品 参数通过事件启用传入</param>
    /// <param name="onBack">事件返回</param>
    public void Show(List<ItemBase> availableItem,Action<ItemBase> onItemSelected,
        Action onBack)
    {
        this.availableItem = availableItem;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;
        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }


    /// <summary>
    /// 更新按键选择逻辑
    /// </summary>
    public void HandleUpdate()
    {
        var prveSelected = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItem.Count - 1);

        if (selectedItem != prveSelected)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
            onItemSelected?.Invoke(availableItem[selectedItem]);
        else if (Input.GetKeyDown(KeyCode.X))
            onBack?.Invoke();
    }

    /// <summary>
    /// 更新商品 列表
    /// </summary>
    void UpdateItemList()
    {
        //清除所有子对象
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        //获得玩家对象下的物品属性
        foreach (var item in availableItem)
        {
            //在（子类，父类） 父类下生成子类
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            //为Ui赋值 设置名字和价格
            slotUIObj.SetNameAndPrice(item);
            //将生成的每一项加入到集合中  用于更新选中效果
            slotUIList.Add(slotUIObj);
        }
        //打开时更新选中的颜色
        UpdateItemSelection();
    }

    /// <summary>
    /// 更新选中效果 颜色 文本 滑动
    /// </summary>
    public void UpdateItemSelection()
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItem.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (selectedItem == i)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if (availableItem.Count > 0)
        {
            var item = availableItem[selectedItem];
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    /// <summary>
    /// 滚动
    /// </summary>
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        //根据当前选定的项目（selectedItem）和视图中可见项目数量的一半（itemsInViewport/2），计算出要滚动的位置（scrollPos）
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        //改变物品集合 的 高度值  //切换的效果
        itemListRect.localPosition = new Vector3(itemListRect.localPosition.x, scrollPos);

        //显示箭头
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

}
