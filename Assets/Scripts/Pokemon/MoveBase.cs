using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Move",menuName ="Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    //威力
    [SerializeField] int power;
    //命中率
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHit;
    [SerializeField] int pp;
    [SerializeField] int priority;
    //类别
    [SerializeField] MoveCategory category;
    //影响
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;


    [SerializeField] AudioClip sound;

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public string Description
    {
        get
        {
            return description;
        }

        set
        {
            description = value;
        }
    }

    public PokemonType Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }

    public int Power
    {
        get
        {
            return power;
        }

        set
        {
            power = value;
        }
    }

    public int Accuracy
    {
        get
        {
            return accuracy;
        }

        set
        {
            accuracy = value;
        }
    }

    public int PP
    {
        get
        {
            return pp;
        }

        set
        {
            pp = value;
        }
    }

    /// <summary>
    /// 攻击技能类别 物理/特殊
    /// </summary>
    public MoveCategory Category
    { get { return category; } }

    /// <summary>
    /// 效果
    /// </summary>
    public MoveEffects Effects
    {
        get
        {
            return effects;
        }
    }

    public MoveTarget Target
    {
        get
        {
            return target;
        }
    }
    /// <summary>
    /// 必定命中
    /// </summary>
    public bool AlwaysHit
    {
        get
        {
            return alwaysHit;
        }

        
    }

    /// <summary>
    /// 第二种状态
    /// </summary>
    public List<SecondaryEffects> Secondaries
    {
        get
        {
            return secondaries;
        }
    }
    
    public int Priority
    {
        get
        {
            return priority;
        }
    }

    public AudioClip Sound => sound;
        

    //改为 MoveCategory
    //public bool IsSpecial
    //{
    //    get
    //    {
    //        if (type == PokemonType.火 || type == PokemonType.水 || type == PokemonType.氷 || type == PokemonType.Electric电
    //          || type == PokemonType.草 || type == PokemonType.龙)
    //        {
    //            return true;
    //        }
    //        else
    //        { return false; }
    //    }

    //}


}

/// <summary>
/// 技能效果
/// </summary>
[System.Serializable]
public class MoveEffects
{

    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;


    /// <summary>
    /// 增幅/降低
    /// </summary>
    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    /// <summary>
    /// 异常状态
    /// </summary>
    public ConditionID Status
    { get { return status; } }

    public ConditionID VolatileStatus
    {
        get
        {
            return volatileStatus;
        }
    }
}

[System.Serializable]
/// <summary>
/// 第二种状态类
/// </summary>
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int change;
    [SerializeField] MoveTarget target;

    /// <summary>
    /// 生效次数
    /// </summary>
    public int Change
    {
        get
        {
            return change;
        }
    }
    /// <summary>
    /// 作用目标
    /// </summary>
    public MoveTarget Target
    {
        get
        {
            return target;
        }
    }
}



    /// <summary>
    /// 提升/降低 类型和等级
    /// </summary>
    [System.Serializable]
public class StatBoost
{
    /// <summary>
    /// 类型
    /// </summary>
    public Stat stat;
    /// <summary>
    /// 等级
    /// </summary>
    public int boost;
}

public enum MoveCategory
{
    Physical,Special,Status
}
public enum MoveTarget
{
    Foe,Self
}