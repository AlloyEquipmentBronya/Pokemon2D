using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;


    public virtual string Name => name;

    public string Description => description;

    public Sprite Icon => icon;

    public float Price => price;
    public bool IsSellable => isSellable;
    

    /// <summary>
    /// 父类 虚方法 使用物品
    /// </summary>
    /// <returns></returns>
    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }
    /// <summary>
    /// 重复使用
    /// </summary>
    public virtual bool IsReusable => false;

    /// <summary>
    /// 战斗中使用
    /// </summary>
    public virtual bool CanUseInBattle => true;
    /// <summary>
    /// 战斗后使用
    /// </summary>
    public virtual bool CanUseOutsideBattle => true;
}
