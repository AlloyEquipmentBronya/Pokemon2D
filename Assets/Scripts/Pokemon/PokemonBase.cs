using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Pokemon",menuName ="Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    //����
    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //��������
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    //����ֵ
    [SerializeField] int expYield;

    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    //���ϴ�ű����ο���ѧϰ���� �������ܺ͵ȼ�
    [SerializeField] List<LearnableMove> learnableMoves;
    //�����ο��Դ�  ��Ʒ���� ��ѧϰ�ļ���
    [SerializeField] List<MoveBase> learnableByItems;
    /// <summary>
    /// ���������б�
    /// </summary>
    [SerializeField] List<Evolution> evolutions;


    public static int MaxNumOfMoves { get; set; } = 4;

    /// <summary>
    /// ������Ҫ�����ľ���ֵ��
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
    /// ������ Ĭ��255
    /// </summary>
    public int CatchRate
    {
        get
        {
            return catchRate;
        }
    }

    /// <summary>
    /// �ɻ�þ���
    /// </summary>
    public int ExpYield
    {
        get
        {
            return expYield;
        }

    }

    /// <summary>
    /// �ɳ�����
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
/// ����ѧϰ�ļ���
/// </summary>
  [System.Serializable]
  public class LearnableMove
{
    //����ѧϰ�ļ���
    [SerializeField] MoveBase moveBase;
    //����ѧϰ�ĵȼ�
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

public enum PokemonType  //����������
{
    None,
    ��ͨ,
    ��,
    ˮ,
    Electric��,
    ��,
    ��,
    ��,
    Poison��,
    ����,
    ����,
    Psychic����,
    Bug��,
    Rock��ʯ,
    ����,
    ��
}


public enum GrowthRate
{
    Fast,MediumFast,
}


/// <summary>
/// ���������� ����/���� ����
/// </summary>
public enum Stat
{
    ����,
    ����,
    �ع�,
    �ط�,
    �ٶ�,

    //�� 2 ������ʵ��ͳ�����ݣ�����������߼��ܾ���
    ������,
    //������
    ������
}

//���Կ�����
public class TypeChart
{
    static float[][] chart =
    {     //                    ��   ��   ˮ   ��   ��   ��   ��   ��   �� 

         /*��*/     new float[]{1f,  1f,  1f,  1f,  1f,  1f,0.5f,  1f,  1f},
         /*��*/     new float[]{1f,0.5f,0.5f,  1f,  2f,  2f,  1f,  1f,  1f},
         /*ˮ*/     new float[]{1f,  2f,0.5f,  1f,0.5f,  1f,  1f,  1f,  2f},
         /*��*/     new float[]{1f,  1f,  2f,0.5f,0.5f,  1f,  1f,  1f,  0f},
         /*��*/     new float[]{1f,0.5f,  2f,  1f,  1f,  1f,  1f,0.5f,  2f},
         /*��*/     new float[]{1f,0.5f,0.5f,  1f,  2f,0.5f,  1f,  1f,  2f},
         /*��*/     new float[]{2f,  1f,  1f,  1f,  1f,  2f,0.5f,0.5f,  1f},
         /*��*/     new float[]{1f,  1f,  1f,  1f,  2f,  1f,  1f,0.5f,0.5f},
         /*��*/     new float[]{1f,  2f,  1f,  1f,0.5f,  1f,  1f,  2f,  1f},
    };

    /// <summary>
    /// ���ؿ��Ʊ���
    /// </summary>
    /// <param name="attacType">�����ļ�������</param>
    /// <param name="defenseType">�������ı���������</param>
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



