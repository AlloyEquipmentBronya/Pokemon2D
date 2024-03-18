using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/����ѧϰ����TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;
    public override bool Use(Pokemon pokemon)
    {
        //ѧϰ�����Ǵӿ��UI�д���ģ��������ѧϰ�ˣ���ô����true
        return pokemon.HasMove(move);
    }

    /// <summary>
    /// �������ܷ�ѧϰ�ü���
    /// </summary>
    /// <param name="pokemon"></param>
    /// <returns></returns>
    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);
    }

    public override string Name => base.Name+$":{move.Name}";

    public override bool CanUseInBattle => false;

    public override bool IsReusable => isHM;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
