using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
       yield return DialogManager.Instance.ShowDialogText("ǰ�����������Կ���");

        var pokemonWithCut = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "ն��"));

        if (pokemonWithCut != null)
        {
            int selectedChoice = 0;

            yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name}Ӧ���ܿ�����",
                choices: new List<string> { "��", "��" },
                onChoiceSelected: (selecttion) => selectedChoice = selecttion);

            if(selectedChoice == 0)
            {
                yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name}��������");
                gameObject.SetActive(false);
            }
        }

    }
}
