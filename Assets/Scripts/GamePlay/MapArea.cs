using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPokemons;


    private void Start()
    {
        int totalChance = 0;
        foreach (var record in wildPokemons)
        {
            record.chanceLower= totalChance;
            record.chanceUpper= totalChance+record.chancePercentage;
            totalChance=totalChance+record.chancePercentage;
        }
    }

    /// <summary>
    /// 返回宝可梦 并给他它始数值 随机生成
    /// </summary>
    /// <returns></returns>
    public Pokemon GetWildPokemon()
    {
        int randVal = Random.Range(1, 101);
        //出现概率的宝可梦
       var pokemonRecord= wildPokemons.First(p=>randVal>=p.chanceLower&&randVal<=p.chanceUpper);
         //随机的等级
      var levelRange=  pokemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        //返回出现的宝可梦
        var wildPokemon = new Pokemon(pokemonRecord.pokemon, level);
       
        //var wildPokemon= wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }


}
/// <summary>
///宝可梦遭遇属性
/// </summary>
[System.Serializable]
public class PokemonEncounterRecord
{
    public PokemonBase pokemon;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set;}
}
