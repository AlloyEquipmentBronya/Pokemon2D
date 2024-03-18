using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/创建恢复物品对象")]
public class RecoverItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive/全恢复")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;


    /// <summary>
    /// 子类 物品使用增加HP
    /// </summary>
    /// <param name="pokemon"></param>
    /// <returns></returns>
    public override bool Use(Pokemon pokemon)
    {
        //复活药水
        if(revive||maxRevive)
        {
            if (pokemon.HP > 0)
                return false;

            if (revive)
                pokemon.IncreaseHP(pokemon.MaxHp / 2);
            else if (maxRevive)
                pokemon.IncreaseHP(pokemon.MaxHp);

            pokemon.CureStatus();

            return true;
        }

        //宝可梦倒下 不能使用恢复类药水
        if (pokemon.HP == 0)
            return false;

        //恢复药水
        if (restoreMaxHP|| hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp)
                return false;

            if (restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else
            pokemon.IncreaseHP(hpAmount);
        }

        //状态恢复药水
        if(recoverAllStatus||status!= ConditionID.无)
        {
            if (pokemon.Stats == null && pokemon.VolatileStatus == null)
                return false;

            if(recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if (pokemon.Status.Id == status)
                    pokemon.CureStatus();
                else if (pokemon.VolatileStatus.Id == status)
                    pokemon.CureVolatileStatus();
                else
                    return false;
            }
        }

        //pp恢复药水
        if (restoreMaxPP)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount > 0)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
