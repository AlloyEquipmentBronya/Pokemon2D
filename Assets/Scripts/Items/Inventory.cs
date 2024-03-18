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
        "物品","精灵球","技能学习"
    };
    /// <summary>
    /// 通过下标返回对应库存类型对象列表
    /// </summary>
    /// <param name="categoryIndex"></param>
    /// <returns></returns>
    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }
    /// <summary>
    /// 获得对应类型的对应下标的物品对象
    /// </summary>
    /// <param name="itemIndex">选中的对应下标</param>
    /// <param name="categoryIndex">对应的类型下标</param>
    /// <returns></returns>
    public ItemBase GetItem(int itemIndex,int categoryIndex)
    {
        var currenSlots = GetSlotsByCategory(categoryIndex);
         return currenSlots[itemIndex].Item;
    }

    /// <summary>
    /// 使用物品 
    /// </summary>
    /// <param name="itemIndex">物品对象下标</param>
    /// <param name="selectedPokemon">宝可梦作用对象</param>
    public ItemBase UseItem(int itemIndex,Pokemon selectedPokemon,int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);

        bool itemUsed = item.Use(selectedPokemon);
        if(itemUsed)
        {
            //物品不是可重复使用时 才删除
            if (!item.IsReusable)
                RemoveItem(item);
            return item;
        }
        return null;
    }

    /// <summary>
    ///添加对应物品
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void AddItem(ItemBase item,int count=1)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //从集合中找到对应 于item 匹配的项  没有返回空
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
        //从集合中找到对应 于item 匹配的项  没有返回空
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        //它的作用是从一个名为 slots 的集合中找到第一个具有特定 item 的 slot
        itemSlot.Count-= countToReMove;
        if (itemSlot.Count == 0)
            currenSlots.Remove(itemSlot);

        onUpdated?.Invoke();
    }
    /// <summary>
    /// 获得物品的数量
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //从集合中找到对应 于item 匹配的项  没有返回空
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        //它的作用是从一个名为 slots 的集合中找到第一个具有特定 item 的 slot

        if (item != null)
            return itemSlot.Count;
        else
            return 0;
    }

    /// <summary>
    /// 是否存在物品
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
    /// 通过物品基类 返回物品对应的枚举类型 
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
    /// 找到PlayerControl的Inventory组件
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
            //获得每一个类型的每一个对象的数据
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
      var saveData=  state as InventorySaveData;
        //通过构造函数为每个物品项的每个物品 赋值数据
        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots };
        //刷新页面
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
    /// 通过构造函数恢复数据
    /// </summary>
    /// <param name="saveData"></param>
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    /// <summary>
    /// 获得要保存的数据
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
/// 保存 数据的名字和数量
/// </summary>
[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

/// <summary>
/// 保存不同物品类型数据
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
}
