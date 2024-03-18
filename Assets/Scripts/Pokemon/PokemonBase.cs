using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Pokemon",menuName ="Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    //描述
    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //基础属性
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    //经验值
    [SerializeField] int expYield;

    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    //集合存放宝可梦可以学习的类 包括技能和等级
    [SerializeField] List<LearnableMove> learnableMoves;
    //宝可梦可以从  物品技能 中学习的集合
    [SerializeField] List<MoveBase> learnableByItems;
    /// <summary>
    /// 进化对象列表
    /// </summary>
    [SerializeField] List<Evolution> evolutions;


    public static int MaxNumOfMoves { get; set; } = 4;

    /// <summary>
    /// 计算需要升级的经验值量
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetExpForLevel(int level)
    {
        if(growthRate==GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if(growthRate==GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        return -1;
    }


    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Defence
    {
        get { return defence; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefence
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }


    public List<LearnableMove> LearnableMoves
    {
        get
        {
            return learnableMoves;
        }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;

    /// <summary>
    /// 捕获率 默认255
    /// </summary>
    public int CatchRate
    {
        get
        {
            return catchRate;
        }
    }

    /// <summary>
    /// 可获得经验
    /// </summary>
    public int ExpYield
    {
        get
        {
            return expYield;
        }

    }

    /// <summary>
    /// 成长速率
    /// </summary>
    public GrowthRate GrowthRate
    {
        get
        {
            return growthRate;
        }
    }

    public List<Evolution> Evolutions => evolutions;
       
}

/// <summary>
/// 可以学习的技能
/// </summary>
  [System.Serializable]
  public class LearnableMove
{
    //可以学习的技能
    [SerializeField] MoveBase moveBase;
    //可以学习的等级
    [SerializeField] int level;

    public MoveBase MoveBase
    {
        get
        {
            return moveBase;
        }

        set
        {
            moveBase = value;
        }
    }

    public int Level
    {
        get
        {
            return level;
        }

        set
        {
            level = value;
        }
    }

}
[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;

    public PokemonBase EvolvesInto => evolvesInto;
    public int ReqquiredLevel => requiredLevel;
    public EvolutionItem RequiredItem => requiredItem;
}

public enum PokemonType  //宝可梦属性
{
    None,
    普通,
    火,
    水,
    Electric电,
    草,
    氷,
    格斗,
    Poison毒,
    地面,
    飞行,
    Psychic超能,
    Bug虫,
    Rock岩石,
    幽灵,
    龙
}


public enum GrowthRate
{
    Fast,MediumFast,
}


/// <summary>
/// 宝可梦属性 提升/降低 类型
/// </summary>
public enum Stat
{
    攻击,
    防御,
    特攻,
    特防,
    速度,

    //这 2 个不是实际统计数据，它们用于提高技能精度
    命中率,
    //闪避率
    闪避率
}

//属性克制类
public class TypeChart
{
    static float[][] chart =
    {     //                    普   火   水   电   草   氷   斗   毒   地 

         /*普*/     new float[]{1f,  1f,  1f,  1f,  1f,  1f,0.5f,  1f,  1f},
         /*火*/     new float[]{1f,0.5f,0.5f,  1f,  2f,  2f,  1f,  1f,  1f},
         /*水*/     new float[]{1f,  2f,0.5f,  1f,0.5f,  1f,  1f,  1f,  2f},
         /*电*/     new float[]{1f,  1f,  2f,0.5f,0.5f,  1f,  1f,  1f,  0f},
         /*草*/     new float[]{1f,0.5f,  2f,  1f,  1f,  1f,  1f,0.5f,  2f},
         /*氷*/     new float[]{1f,0.5f,0.5f,  1f,  2f,0.5f,  1f,  1f,  2f},
         /*斗*/     new float[]{2f,  1f,  1f,  1f,  1f,  2f,0.5f,0.5f,  1f},
         /*毒*/     new float[]{1f,  1f,  1f,  1f,  2f,  1f,  1f,0.5f,0.5f},
         /*地*/     new float[]{1f,  2f,  1f,  1f,0.5f,  1f,  1f,  2f,  1f},
    };

    /// <summary>
    /// 返回克制倍率
    /// </summary>
    /// <param name="attacType">攻击的技能类型</param>
    /// <param name="defenseType">被攻击的宝可梦类型</param>
    /// <returns></returns>
    public static float GetEffectiveness(PokemonType attacType,PokemonType defenseType)
    {
        if (attacType == PokemonType.None || defenseType == PokemonType.None)
            return 1;
        int row = (int)attacType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }

   // public string Gets
}



