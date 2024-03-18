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
    //����
    [SerializeField] int power;
    //������
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHit;
    [SerializeField] int pp;
    [SerializeField] int priority;
    //���
    [SerializeField] MoveCategory category;
    //Ӱ��
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
    /// ����������� ����/����
    /// </summary>
    public MoveCategory Category
    { get { return category; } }

    /// <summary>
    /// Ч��
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
    /// �ض�����
    /// </summary>
    public bool AlwaysHit
    {
        get
        {
            return alwaysHit;
        }

        
    }

    /// <summary>
    /// �ڶ���״̬
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
        

    //��Ϊ MoveCategory
    //public bool IsSpecial
    //{
    //    get
    //    {
    //        if (type == PokemonType.�� || type == PokemonType.ˮ || type == PokemonType.�� || type == PokemonType.Electric��
    //          || type == PokemonType.�� || type == PokemonType.��)
    //        {
    //            return true;
    //        }
    //        else
    //        { return false; }
    //    }

    //}


}

/// <summary>
/// ����Ч��
/// </summary>
[System.Serializable]
public class MoveEffects
{

    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;


    /// <summary>
    /// ����/����
    /// </summary>
    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    /// <summary>
    /// �쳣״̬
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
/// �ڶ���״̬��
/// </summary>
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int change;
    [SerializeField] MoveTarget target;

    /// <summary>
    /// ��Ч����
    /// </summary>
    public int Change
    {
        get
        {
            return change;
        }
    }
    /// <summary>
    /// ����Ŀ��
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
    /// ����/���� ���ͺ͵ȼ�
    /// </summary>
    [System.Serializable]
public class StatBoost
{
    /// <summary>
    /// ����
    /// </summary>
    public Stat stat;
    /// <summary>
    /// �ȼ�
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