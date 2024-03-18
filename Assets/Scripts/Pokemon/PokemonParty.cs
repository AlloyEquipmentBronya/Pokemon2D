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
    /// ���ر�����HP�д���0 ��Ϊlist���ϵ�һ�� 
    /// </summary>
    /// <returns></returns>
    public Pokemon GetHealthyPokemon()
    {
        //����  ���ر������д���0 ��Ϊlist���ϵ�һ�� 
        return Pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }


    /// <summary>
    /// �������
    /// </summary>
    /// <param name="newPokemon"></param>
    public void AddPokemon(Pokemon newPokemon)
    {
        if(pokemons.Count<6)
        {
            pokemons.Add(newPokemon);

            //���������ʱ��������
            OnUpdated?.Invoke();
        }
        else
        {
            //Ҫ���������
        }
    }

    /// <summary>
    /// ��ⱦ���ζ������Ƿ��пɽ�����
    /// </summary>
    /// <returns></returns>
    public bool CheckForEvolution()
    {
        //���������Լ����е�ÿ��Ԫ�أ������� Pokemon ���� p������ CheckForEvolution() ������
        //�ж��䷵��ֵ�Ƿ�Ϊ null��������κ�һ��Ԫ������������Any() �����ͻ᷵�� true�����򷵻� false��
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    /// <summary>
    /// ִ�б����ν���
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
    /// ����UI ����
    /// </summary>
    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }



    /// <summary>
    /// ���PlayerControl �µ�PokemonParty���
    /// Ҳ������ҵı����ζ���
    /// </summary>
    /// <returns></returns>
    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerControl>().GetComponent<PokemonParty>();
    }
}
