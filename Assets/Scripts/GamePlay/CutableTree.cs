using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
       yield return DialogManager.Instance.ShowDialogText("前面的树好像可以砍掉");

        var pokemonWithCut = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "斩切"));

        if (pokemonWithCut != null)
        {
            int selectedChoice = 0;

            yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name}应该能砍掉树",
                choices: new List<string> { "是", "否" },
                onChoiceSelected: (selecttion) => selectedChoice = selecttion);

            if(selectedChoice == 0)
            {
                yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name}砍掉树了");
                gameObject.SetActive(false);
            }
        }

    }
}
