using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/创建宝可梦球对象")]
public class PokeballItem :ItemBase
{
    [SerializeField] float catchRateModfier = 1;

    public override bool Use(Pokemon pokemon)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModfier => catchRateModfier;
}
