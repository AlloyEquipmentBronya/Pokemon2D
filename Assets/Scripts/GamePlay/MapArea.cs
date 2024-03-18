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
    /// ���ر����� ��������ʼ��ֵ �������
    /// </summary>
    /// <returns></returns>
    public Pokemon GetWildPokemon()
    {
        int randVal = Random.Range(1, 101);
        //���ָ��ʵı�����
       var pokemonRecord= wildPokemons.First(p=>randVal>=p.chanceLower&&randVal<=p.chanceUpper);
         //����ĵȼ�
      var levelRange=  pokemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        //���س��ֵı�����
        var wildPokemon = new Pokemon(pokemonRecord.pokemon, level);
       
        //var wildPokemon= wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }


}
/// <summary>
///��������������
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
