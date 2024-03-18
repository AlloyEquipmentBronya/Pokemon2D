using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB : MonoBehaviour
{
    /// <summary>
    /// �����UI����ʾ��״̬���� key Ϊcondition��ConditionID ��ֵ
    /// ��condition�ܵ��ó� ConditionDB�µ�ConditionID����
    /// </summary>
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }


    //�ֵ� �洢 �쳣״̬
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
        ConditionID.�ж�,
            new Condition()
            {
                Name ="�ж�",
                StartMessage="�ж���",
                OnAfterTurn=(Pokemon pokemon)=>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}��Ϊ�ж��ܵ��˺�");
                }
            }

        },
        {
        ConditionID.����,
            new Condition()
            {
                Name ="����",
                StartMessage="������",
                OnAfterTurn=(Pokemon pokemon)=>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}��Ϊ�����ܵ��˺�");
                }
            }

        },
        {
        ConditionID.���,
            new Condition()
            {
                Name ="���",
                StartMessage="�����",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                   if(Random.Range(1,5)==1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}������޷�����");
                        return false;
                    }
                    return true;
                }
            }

        },
        {
             ConditionID.����,
            new Condition()
            {
                Name ="����",
                StartMessage="��ס��",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    //��������
                   if(Random.Range(1,5)==1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}�Ķ���״̬�����");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
             ConditionID.����,
            new Condition()
            {
                Name ="����",
                StartMessage="˯����",
                OnStart=(Pokemon pokemon)=>
                {
                     //1-3�غ�
                     pokemon.StatusTime=Random.Range(1,4);
                    Debug.Log($"����˯{pokemon.StatusTime}�غ�");
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    //˯�� 
                    if(pokemon.StatusTime<=0)
                    {
                        pokemon.CureStatus();
                         pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}����");
                        return true;
                    }
                    //ÿ˯һ�غϼ���һ�غ�
                   pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}˯����");
                    return false;
                }
            }
        },

        {
             ConditionID.����,
            new Condition()
            {
                Name ="����",
                StartMessage="������",
                OnStart=(Pokemon pokemon)=>
                {
                     //1-4�غ�
                     pokemon.VolatileStatusTime=Random.Range(1,5);
                     Debug.Log($"�������{pokemon.VolatileStatusTime}�غ�");
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    if(pokemon.VolatileStatusTime<=0)
                    {
                        pokemon.CureVolatileStatus();
                         pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}���ҽ����");
                        return true;
                    }
                    //ÿһ�غϼ���һ�غ�
                   pokemon.VolatileStatusTime--;

                    if(Random.Range(1,3)==1)
                     return true;

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}������");
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                   pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}�����˹������Լ�");
                    return false;
                }
            }
        },

    };


    /// <summary>
    /// ״̬�쳣�������
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.���� || condition.Id == ConditionID.����)
            return 2f;
        else if (condition.Id == ConditionID.��� || condition.Id == ConditionID.�ж� || condition.Id == ConditionID.����)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    ��,�ж�,����,����,���,����,����
}
