using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/�����ָ���Ʒ����")]
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

    [Header("Revive/ȫ�ָ�")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;


    /// <summary>
    /// ���� ��Ʒʹ������HP
    /// </summary>
    /// <param name="pokemon"></param>
    /// <returns></returns>
    public override bool Use(Pokemon pokemon)
    {
        //����ҩˮ
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

        //�����ε��� ����ʹ�ûָ���ҩˮ
        if (pokemon.HP == 0)
            return false;

        //�ָ�ҩˮ
        if (restoreMaxHP|| hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp)
                return false;

            if (restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else
            pokemon.IncreaseHP(hpAmount);
        }

        //״̬�ָ�ҩˮ
        if(recoverAllStatus||status!= ConditionID.��)
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

        //pp�ָ�ҩˮ
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
