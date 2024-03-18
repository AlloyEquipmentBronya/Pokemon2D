using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB : MonoBehaviour
{
    /// <summary>
    /// 获得在UI中显示的状态名称 key 为condition的ConditionID 赋值
    /// 让condition能调用出 ConditionDB下的ConditionID名称
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


    //字典 存储 异常状态
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
        ConditionID.中毒,
            new Condition()
            {
                Name ="中毒",
                StartMessage="中毒了",
                OnAfterTurn=(Pokemon pokemon)=>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}因为中毒受到伤害");
                }
            }

        },
        {
        ConditionID.灼烧,
            new Condition()
            {
                Name ="烧伤",
                StartMessage="烧伤了",
                OnAfterTurn=(Pokemon pokemon)=>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}因为烧伤受到伤害");
                }
            }

        },
        {
        ConditionID.麻痹,
            new Condition()
            {
                Name ="麻痹",
                StartMessage="麻痹了",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                   if(Random.Range(1,5)==1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}麻痹了无法动弹");
                        return false;
                    }
                    return true;
                }
            }

        },
        {
             ConditionID.冰冻,
            new Condition()
            {
                Name ="冻结",
                StartMessage="冻住了",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    //自愈概率
                   if(Random.Range(1,5)==1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}的冻结状态解除了");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
             ConditionID.催眠,
            new Condition()
            {
                Name ="催眠",
                StartMessage="睡着了",
                OnStart=(Pokemon pokemon)=>
                {
                     //1-3回合
                     pokemon.StatusTime=Random.Range(1,4);
                    Debug.Log($"将会睡{pokemon.StatusTime}回合");
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    //睡醒 
                    if(pokemon.StatusTime<=0)
                    {
                        pokemon.CureStatus();
                         pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}醒了");
                        return true;
                    }
                    //每睡一回合减少一回合
                   pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}睡着了");
                    return false;
                }
            }
        },

        {
             ConditionID.混乱,
            new Condition()
            {
                Name ="混乱",
                StartMessage="混乱了",
                OnStart=(Pokemon pokemon)=>
                {
                     //1-4回合
                     pokemon.VolatileStatusTime=Random.Range(1,5);
                     Debug.Log($"将会混乱{pokemon.VolatileStatusTime}回合");
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    if(pokemon.VolatileStatusTime<=0)
                    {
                        pokemon.CureVolatileStatus();
                         pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}混乱解除了");
                        return true;
                    }
                    //每一回合减少一回合
                   pokemon.VolatileStatusTime--;

                    if(Random.Range(1,3)==1)
                     return true;

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}混乱了");
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                   pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}混乱了攻击了自己");
                    return false;
                }
            }
        },

    };


    /// <summary>
    /// 状态异常捕获概率
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.催眠 || condition.Id == ConditionID.冰冻)
            return 2f;
        else if (condition.Id == ConditionID.麻痹 || condition.Id == ConditionID.中毒 || condition.Id == ConditionID.灼烧)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    无,中毒,灼烧,催眠,麻痹,冰冻,混乱
}
