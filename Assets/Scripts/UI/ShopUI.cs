using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{

    //�Ӷ��� ��Ʒ UI�ϼ�
    [SerializeField] GameObject itemList;
    //Ϊ UI��ֵ����Ʒ��
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
    /// ��ӵĻ���
    /// </summary>
    [SerializeField] List<ItemBase> availableItem;


    const int itemsInViewport = 10;
    RectTransform itemListRect;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }


    /// <summary>
    /// ��ʾ�̵�����б�
    /// </summary>
    /// <param name="availableItem">�ɹ���/ѡ�����Ʒ�б�</param>
    /// <param name="onItemSelected">�¼�ѡ����Ʒ ����ͨ���¼����ô���</param>
    /// <param name="onBack">�¼�����</param>
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
    /// ���°���ѡ���߼�
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
    /// ������Ʒ �б�
    /// </summary>
    void UpdateItemList()
    {
        //��������Ӷ���
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        //�����Ҷ����µ���Ʒ����
        foreach (var item in availableItem)
        {
            //�ڣ����࣬���ࣩ ��������������
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            //ΪUi��ֵ �������ֺͼ۸�
            slotUIObj.SetNameAndPrice(item);
            //�����ɵ�ÿһ����뵽������  ���ڸ���ѡ��Ч��
            slotUIList.Add(slotUIObj);
        }
        //��ʱ����ѡ�е���ɫ
        UpdateItemSelection();
    }

    /// <summary>
    /// ����ѡ��Ч�� ��ɫ �ı� ����
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
    /// ����
    /// </summary>
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        //���ݵ�ǰѡ������Ŀ��selectedItem������ͼ�пɼ���Ŀ������һ�루itemsInViewport/2���������Ҫ������λ�ã�scrollPos��
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        //�ı���Ʒ���� �� �߶�ֵ  //�л���Ч��
        itemListRect.localPosition = new Vector3(itemListRect.localPosition.x, scrollPos);

        //��ʾ��ͷ
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

}
