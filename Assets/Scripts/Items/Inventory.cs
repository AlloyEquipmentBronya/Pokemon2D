using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum ItemCategory { Items,Pokeballs,Tms};

public class Inventory : MonoBehaviour,ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    List<List<ItemSlot>> allSlots;

    public event Action onUpdated; 


    private void Awake()
    {
        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "��Ʒ","������","����ѧϰ"
    };
    /// <summary>
    /// ͨ���±귵�ض�Ӧ������Ͷ����б�
    /// </summary>
    /// <param name="categoryIndex"></param>
    /// <returns></returns>
    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }
    /// <summary>
    /// ��ö�Ӧ���͵Ķ�Ӧ�±����Ʒ����
    /// </summary>
    /// <param name="itemIndex">ѡ�еĶ�Ӧ�±�</param>
    /// <param name="categoryIndex">��Ӧ�������±�</param>
    /// <returns></returns>
    public ItemBase GetItem(int itemIndex,int categoryIndex)
    {
        var currenSlots = GetSlotsByCategory(categoryIndex);
         return currenSlots[itemIndex].Item;
    }

    /// <summary>
    /// ʹ����Ʒ 
    /// </summary>
    /// <param name="itemIndex">��Ʒ�����±�</param>
    /// <param name="selectedPokemon">���������ö���</param>
    public ItemBase UseItem(int itemIndex,Pokemon selectedPokemon,int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);

        bool itemUsed = item.Use(selectedPokemon);
        if(itemUsed)
        {
            //��Ʒ���ǿ��ظ�ʹ��ʱ ��ɾ��
            if (!item.IsReusable)
                RemoveItem(item);
            return item;
        }
        return null;
    }

    /// <summary>
    ///��Ӷ�Ӧ��Ʒ
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void AddItem(ItemBase item,int count=1)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //�Ӽ������ҵ���Ӧ ��item ƥ�����  û�з��ؿ�
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        if(itemSlot!=null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currenSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }
        onUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item,int countToReMove=1)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //�Ӽ������ҵ���Ӧ ��item ƥ�����  û�з��ؿ�
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        //���������Ǵ�һ����Ϊ slots �ļ������ҵ���һ�������ض� item �� slot
        itemSlot.Count-= countToReMove;
        if (itemSlot.Count == 0)
            currenSlots.Remove(itemSlot);

        onUpdated?.Invoke();
    }
    /// <summary>
    /// �����Ʒ������
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //�Ӽ������ҵ���Ӧ ��item ƥ�����  û�з��ؿ�
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        //���������Ǵ�һ����Ϊ slots �ļ������ҵ���һ�������ض� item �� slot

        if (item != null)
            return itemSlot.Count;
        else
            return 0;
    }

    /// <summary>
    /// �Ƿ������Ʒ
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);

        return currenSlots.Exists(slots=> slots.Item == item);
    }

    /// <summary>
    /// ͨ����Ʒ���� ������Ʒ��Ӧ��ö������ 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    ItemCategory GetCategoryFormItem(ItemBase item)
    {
        if (item is RecoverItem||item is EvolutionItem)
            return ItemCategory.Items;
        else if (item is PokeballItem)
            return ItemCategory.Pokeballs;
        else 
            return ItemCategory.Tms;
    }
    /// <summary>
    /// �ҵ�PlayerControl��Inventory���
    /// </summary>
    /// <returns></returns>
    public static Inventory GetInventory()
    {
       return FindObjectOfType<PlayerControl>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            //���ÿһ�����͵�ÿһ�����������
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
      var saveData=  state as InventorySaveData;
        //ͨ�����캯��Ϊÿ����Ʒ���ÿ����Ʒ ��ֵ����
        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots };
        //ˢ��ҳ��
        onUpdated?.Invoke();
    }

}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }
    /// <summary>
    /// ͨ�����캯���ָ�����
    /// </summary>
    /// <param name="saveData"></param>
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    /// <summary>
    /// ���Ҫ���������
    /// </summary>
    /// <returns></returns>
    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };
        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }

    public int Count
    {
        get => count;
        set => count = value;
    }
}

/// <summary>
/// ���� ���ݵ����ֺ�����
/// </summary>
[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

/// <summary>
/// ���治ͬ��Ʒ��������
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
}
