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
    /// ���� �鷽�� ʹ����Ʒ
    /// </summary>
    /// <returns></returns>
    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }
    /// <summary>
    /// �ظ�ʹ��
    /// </summary>
    public virtual bool IsReusable => false;

    /// <summary>
    /// ս����ʹ��
    /// </summary>
    public virtual bool CanUseInBattle => true;
    /// <summary>
    /// ս����ʹ��
    /// </summary>
    public virtual bool CanUseOutsideBattle => true;
}
