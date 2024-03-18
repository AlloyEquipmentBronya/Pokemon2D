using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManger : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    [SerializeField] AudioClip evolutionMusic;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManger i { get; private set; }

    private void Awake()
    {
        i = this;
    }
    /// <summary>
    /// ����ִ��
    /// </summary>
    /// <param name="pokemon">ִ�б�����</param>
    /// <param name="evolution">��������</param>
    /// <returns></returns>
    public IEnumerator Evolve(Pokemon pokemon,Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        //������������ ִ��
        AudioManager.i.PlayMusic(evolutionMusic);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}������");

        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);


        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name}��������{pokemon.Base.Name}");

        evolutionUI.SetActive(false);

        OnCompleteEvolution?.Invoke();
    }
}
