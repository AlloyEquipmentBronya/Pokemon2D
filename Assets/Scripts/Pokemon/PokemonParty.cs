using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
        set
        {
            pokemons = value;

            OnUpdated.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in Pokemons)
        {
            pokemon.Init();
        }
    }
    private void Start()
    {
    }
    /// <summary>
    /// 返回宝可梦HP中大于0 且为list集合第一个 
    /// </summary>
    /// <returns></returns>
    public Pokemon GetHealthyPokemon()
    {
        //链接  返回宝可梦中大于0 且为list集合第一个 
        return Pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }


    /// <summary>
    /// 加入队伍
    /// </summary>
    /// <param name="newPokemon"></param>
    public void AddPokemon(Pokemon newPokemon)
    {
        if(pokemons.Count<6)
        {
            pokemons.Add(newPokemon);

            //宝可梦添加时更新数据
            OnUpdated?.Invoke();
        }
        else
        {
            //要加入电脑中
        }
    }

    /// <summary>
    /// 检测宝可梦队伍中是否有可进化的
    /// </summary>
    /// <returns></returns>
    public bool CheckForEvolution()
    {
        //这个条件会对集合中的每个元素（这里是 Pokemon 对象 p）调用 CheckForEvolution() 方法，
        //判断其返回值是否不为 null。如果有任何一个元素满足条件，Any() 方法就会返回 true，否则返回 false。
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    /// <summary>
    /// 执行宝可梦进化
    /// </summary>
    /// <returns></returns>
    public IEnumerator RunEvolution()
    {
        foreach (var pokemon in pokemons)
        {
            var evoution = pokemon.CheckForEvolution();
            if (evoution != null)
            {
             yield return EvolutionManger.i.Evolve(pokemon, evoution);
            }
        }
    }
    /// <summary>
    /// 更新UI 界面
    /// </summary>
    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }



    /// <summary>
    /// 获得PlayerControl 下的PokemonParty组件
    /// 也就是玩家的宝可梦队伍
    /// </summary>
    /// <returns></returns>
    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerControl>().GetComponent<PokemonParty>();
    }
}
