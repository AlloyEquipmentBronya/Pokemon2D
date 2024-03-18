using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/创建进化物品")]
public class EvolutionItem :ItemBase
{
    public override bool Use(Pokemon pokemon)
    {
        return true;
    }
}
