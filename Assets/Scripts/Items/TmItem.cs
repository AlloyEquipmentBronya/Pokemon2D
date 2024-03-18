using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/创建学习技能TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;
    public override bool Use(Pokemon pokemon)
    {
        //学习技能是从库存UI中处理的，如果它被学习了，那么返回true
        return pokemon.HasMove(move);
    }

    /// <summary>
    /// 宝可梦能否学习该技能
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
